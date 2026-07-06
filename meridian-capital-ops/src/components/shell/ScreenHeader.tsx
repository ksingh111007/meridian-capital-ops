/** Standard screen header row: title (+ context note) left, actions right. */
export function ScreenHeader({ title, context, children }: { title: React.ReactNode; context?: string; children?: React.ReactNode }) {
  return (
    <div className="flex flex-wrap items-center justify-between gap-3 border-b border-line px-5 py-3">
      <h1 className="flex items-baseline gap-2 text-[15px] font-bold text-ink">
        {title}
        {context && <span className="text-[11px] font-medium text-ink-faint">· {context}</span>}
      </h1>
      {children && <div className="flex items-center gap-2">{children}</div>}
    </div>
  );
}
