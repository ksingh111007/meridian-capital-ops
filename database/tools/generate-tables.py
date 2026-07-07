#!/usr/bin/env python3
"""Regenerates database/src table definitions from the EF Core model.

Flow (run from database/tools):
    dotnet run --project SchemaExport > /tmp/efcreate.sql
    python3 generate-tables.py /tmp/efcreate.sql

The EF model (backend AppDbContext, SqlServer provider) is the source of truth
for schemas, tables, columns, defaults, keys, foreign keys, and indexes. This
script adds what EF does not know about:
  - system-versioning: hidden ValidFrom/ValidTo period columns and
    SYSTEM_VERSIONING = ON with a named history table in the [hist] schema
    (one per base table, auto-created by sqlpackage);
  - one .sql file per table under src/<schema>/Tables/, with its indexes.
Existing files are rewritten in place — check the diff after regenerating.
"""

import re
import sys
from pathlib import Path

DATABASE_ROOT = Path(__file__).resolve().parent.parent
SRC = DATABASE_ROOT / "src"

PERIOD_COLUMNS = """    [ValidFrom] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [ValidTo] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])"""

HEADER = "-- Generated from the EF Core model by tools/generate-tables.py — edit the model, then regenerate.\n"


def main(create_script: Path) -> None:
    statements = [s.strip() for s in re.split(r"^GO\s*$", create_script.read_text(), flags=re.M) if s.strip()]

    schemas: list[str] = []
    tables: dict[tuple[str, str], str] = {}
    indexes: dict[tuple[str, str], list[str]] = {}

    for statement in statements:
        if statement.startswith("IF SCHEMA_ID"):
            schemas.append(re.search(r"CREATE SCHEMA \[(\w+)\]", statement).group(1))
        elif statement.startswith("CREATE TABLE"):
            schema, table = re.match(r"CREATE TABLE \[(\w+)\]\.\[(\w+)\]", statement).groups()
            tables[(schema, table)] = to_temporal(statement, schema, table)
        elif "INDEX" in statement.split("\n")[0]:
            schema, table = re.search(r"ON \[(\w+)\]\.\[(\w+)\]", statement).groups()
            indexes.setdefault((schema, table), []).append(statement)

    schemas_dir = SRC / "Schemas"
    schemas_dir.mkdir(parents=True, exist_ok=True)
    for schema in sorted(schemas) + ["hist"]:
        (schemas_dir / f"{schema}.sql").write_text(f"CREATE SCHEMA [{schema}];\n", encoding="utf-8")

    for (schema, table), definition in tables.items():
        table_dir = SRC / schema / "Tables"
        table_dir.mkdir(parents=True, exist_ok=True)
        parts = [definition] + [f"GO\n{index}" for index in indexes.get((schema, table), [])]
        (table_dir / f"{table}.sql").write_text(HEADER + "\n".join(parts) + "\n", encoding="utf-8")

    print(f"Wrote {len(schemas) + 1} schemas and {len(tables)} tables under {SRC}")


def to_temporal(statement: str, schema: str, table: str) -> str:
    lines = statement.rstrip().removesuffix(");").rstrip().split("\n")
    first_constraint = next(
        (i for i, line in enumerate(lines) if line.lstrip().startswith("CONSTRAINT ")), len(lines))
    lines[first_constraint - 1] = lines[first_constraint - 1].rstrip(",") + ","
    lines.insert(first_constraint, PERIOD_COLUMNS + ("," if first_constraint < len(lines) else ""))
    body = "\n".join(lines)
    return (
        f"{body}\n)\n"
        f"WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [hist].[{schema}_{table}]));"
    )


if __name__ == "__main__":
    main(Path(sys.argv[1]))
