namespace Meridian.Application.Abstractions;

/// <summary>
/// Who to stamp into row-level audit columns (CreatedBy/ModifiedBy). The API host
/// resolves the authenticated principal; background jobs and seeding fall back to
/// "system".
/// </summary>
public interface IAuditActorProvider
{
    string CurrentActor { get; }
}
