"use client";

import { createContext, useCallback, useContext, useRef, useState } from "react";

interface Toast {
  id: number;
  kind: "success" | "error";
  title: string;
  detail?: string;
  actionLabel?: string; // "Undo" / "Retry"
  onAction?: () => void;
}

const ToastContext = createContext<{ push: (t: Omit<Toast, "id">) => void } | null>(null);

/** Every mutation shows a toast (pattern 7b): success with Undo where safe, error with Retry. */
export function useToast() {
  const ctx = useContext(ToastContext);
  if (!ctx) throw new Error("useToast must be used inside <ToastProvider>");
  return ctx;
}

export function ToastProvider({ children }: { children: React.ReactNode }) {
  const [toasts, setToasts] = useState<Toast[]>([]);
  const nextId = useRef(1);

  const push = useCallback((t: Omit<Toast, "id">) => {
    const id = nextId.current++;
    setToasts((prev) => [...prev, { ...t, id }]);
    setTimeout(() => setToasts((prev) => prev.filter((x) => x.id !== id)), 6000);
  }, []);

  return (
    <ToastContext.Provider value={{ push }}>
      {children}
      <div aria-live="polite" className="pointer-events-none fixed bottom-4 right-4 z-[60] flex w-80 flex-col gap-2">
        {toasts.map((t) => (
          <div
            key={t.id}
            className={`pointer-events-auto flex items-center gap-2.5 rounded-lg border border-line bg-card px-3 py-2.5 shadow-pop border-l-4 ${
              t.kind === "success" ? "border-l-positive" : "border-l-danger"
            }`}
          >
            <span className={`flex h-5 w-5 flex-none items-center justify-center rounded-full text-[11px] font-bold text-white ${t.kind === "success" ? "bg-positive" : "bg-danger"}`}>
              {t.kind === "success" ? "✓" : "!"}
            </span>
            <div className="min-w-0 flex-1 text-xs">
              <b className="text-ink">{t.title}</b>
              {t.detail && <div className="truncate text-[11px] text-ink-muted">{t.detail}</div>}
            </div>
            {t.actionLabel && (
              <button
                type="button"
                className="text-[11px] font-semibold text-primary hover:underline"
                onClick={() => { t.onAction?.(); setToasts((prev) => prev.filter((x) => x.id !== t.id)); }}
              >
                {t.actionLabel}
              </button>
            )}
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  );
}
