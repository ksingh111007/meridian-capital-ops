"use client";

/** Filter chip row. Controlled: pass `active` + `onChange`. */
export function FilterChips({
  options, active, onChange, trailing,
}: {
  options: string[];
  active: string;
  onChange: (value: string) => void;
  trailing?: React.ReactNode;
}) {
  return (
    <div className="flex flex-wrap items-center gap-2 border-b border-line px-5 py-2.5">
      {options.map((o) => (
        <button
          key={o}
          type="button"
          aria-pressed={o === active}
          onClick={() => onChange(o)}
          className={`rounded-full border px-3 py-1 text-[11px] font-medium transition-colors ${
            o === active ? "border-ink bg-ink text-white" : "border-line-strong bg-card text-ink-secondary hover:border-ink"
          }`}
        >
          {o}
        </button>
      ))}
      {trailing && <span className="ml-auto text-[11px] text-ink-faint">{trailing}</span>}
    </div>
  );
}

export function Tabs({ tabs, active, onChange }: { tabs: string[]; active: string; onChange: (tab: string) => void }) {
  return (
    <div role="tablist" className="flex gap-1 border-b border-line px-5 pt-2">
      {tabs.map((t) => (
        <button
          key={t}
          role="tab"
          aria-selected={t === active}
          type="button"
          onClick={() => onChange(t)}
          className={`-mb-px rounded-t-md border-x border-t px-3.5 py-1.5 text-xs font-semibold ${
            t === active ? "border-line bg-card text-ink" : "border-transparent text-ink-muted hover:text-ink"
          }`}
        >
          {t}
        </button>
      ))}
    </div>
  );
}

export function Toggle({ on, onChange, label, disabled }: { on: boolean; onChange?: (on: boolean) => void; label?: string; disabled?: boolean }) {
  return (
    <button
      type="button"
      role="switch"
      aria-checked={on}
      aria-label={label}
      disabled={disabled}
      onClick={() => onChange?.(!on)}
      className={`relative h-[18px] w-8 flex-none rounded-full transition-colors disabled:opacity-40 ${on ? "bg-primary" : "bg-line-strong"}`}
    >
      <span className={`absolute top-0.5 h-3.5 w-3.5 rounded-full bg-white shadow transition-all ${on ? "left-[15px]" : "left-0.5"}`} />
    </button>
  );
}

export function SearchInput({ placeholder = "Search…", value, onChange }: { placeholder?: string; value: string; onChange: (v: string) => void }) {
  return (
    <input
      type="search"
      value={value}
      placeholder={placeholder}
      onChange={(e) => onChange(e.target.value)}
      className="h-7 w-40 rounded-md border border-line-strong bg-card px-2.5 text-xs text-ink placeholder:text-ink-faint focus:border-primary focus:outline-none"
    />
  );
}

// ---------- form fields ----------

export function Field({ label, required, children, hint }: { label: string; required?: boolean; children: React.ReactNode; hint?: string }) {
  return (
    <label className="block">
      <span className="mb-1 block text-[10.5px] font-semibold uppercase tracking-wider text-ink-faint">
        {label} {required && <span className="text-danger">*</span>}
      </span>
      {children}
      {hint && <span className="mt-1 block text-[11px] text-ink-muted">{hint}</span>}
    </label>
  );
}

const inputCls =
  "w-full rounded-md border border-line-strong bg-card px-2.5 py-1.5 text-xs text-ink placeholder:text-ink-faint focus:border-primary focus:outline-none";

export function TextInput(props: React.InputHTMLAttributes<HTMLInputElement>) {
  return <input {...props} className={`${inputCls} ${props.className ?? ""}`} />;
}

export function TextArea(props: React.TextareaHTMLAttributes<HTMLTextAreaElement>) {
  return <textarea rows={4} {...props} className={`${inputCls} resize-none ${props.className ?? ""}`} />;
}

export function Select({ options, ...props }: React.SelectHTMLAttributes<HTMLSelectElement> & { options: string[] }) {
  return (
    <select {...props} className={`${inputCls} ${props.className ?? ""}`}>
      {options.map((o) => (
        <option key={o} value={o}>{o}</option>
      ))}
    </select>
  );
}

/** none / V / E / A capability picker (admin edit-role dialog). */
export function SegmentPicker({ value, onChange, segments = ["—", "V", "E", "A"] }: { value: string; onChange: (v: string) => void; segments?: string[] }) {
  return (
    <span className="inline-flex overflow-hidden rounded-md border border-line-strong">
      {segments.map((s, i) => (
        <button
          key={s}
          type="button"
          aria-pressed={s === value}
          onClick={() => onChange(s)}
          className={`px-2 py-0.5 font-mono text-[10px] font-bold ${i > 0 ? "border-l border-line" : ""} ${
            s === value ? "bg-primary text-white" : "bg-card text-ink-muted hover:bg-fill"
          }`}
        >
          {s}
        </button>
      ))}
    </span>
  );
}
