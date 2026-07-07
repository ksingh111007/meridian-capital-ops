import type { Metadata } from "next";
import { getUsersAndRoles } from "@/lib/data";
import { UsersRolesScreen } from "@/screens/admin/UsersRolesScreen";

export const metadata: Metadata = { title: "Users & Roles" };

/** Screen 5a — seats, RBAC, and the role → capability matrix. */
export default async function UsersRolesPage() {
  const { kpis, users, roles } = await getUsersAndRoles();
  return <UsersRolesScreen kpis={kpis} users={users} roles={roles} />;
}
