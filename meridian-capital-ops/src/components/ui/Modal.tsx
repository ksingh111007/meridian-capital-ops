"use client";

import { useEffect, useRef } from "react";
import { Button } from "./Button";

/** Form/dialog modal with focus trap, ESC + overlay close. */
export function Modal({
  open, onClose, title, children, footer, width = 420,
}: {
  open: boolean;
  onClose: () => void;
  title: React.ReactNode;
  children: React.ReactNode;
  footer?: React.ReactNode;
  width?: number;
}) {
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!open) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape") onClose();
      if (e.key === "Tab" && ref.current) {
        const focusables = ref.current.querySelectorAll<HTMLElement>("button, input, select, textarea, a[href]");
        if (focusables.length === 0) return;
        const first = focusables[0], last = focusables[focusables.length - 1];
        if (e.shiftKey && document.activeElement === first) { e.preventDefault(); last.focus(); }
        else if (!e.shiftKey && document.activeElement === last) { e.preventDefault(); first.focus(); }
      }
    };
    document.addEventListener("keydown", onKey);
    ref.current?.querySelector<HTMLElement>("input, select, textarea, button")?.focus();
    return () => document.removeEventListener("keydown", onKey);
  }, [open, onClose]);

  if (!open) return null;
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-ink/40 p-4" onMouseDown={(e) => e.target === e.currentTarget && onClose()}>
      <div ref={ref} role="dialog" aria-modal="true" className="max-h-[90vh] w-full overflow-auto rounded-xl border border-line bg-card shadow-pop" style={{ maxWidth: width }}>
        <div className="flex items-center justify-between border-b border-line px-4 py-3">
          <h2 className="text-sm font-bold text-ink">{title}</h2>
          <button type="button" aria-label="Close" onClick={onClose} className="text-base leading-none text-ink-faint hover:text-ink">×</button>
        </div>
        <div className="px-4 py-4">{children}</div>
        {footer && <div className="flex justify-end gap-2 border-t border-line bg-fill px-4 py-3">{footer}</div>}
      </div>
    </div>
  );
}

/** Confirm dialog for destructive / workflow-affecting actions (pattern 7b). */
export function ConfirmDialog({
  open, onCancel, onConfirm, title, body, confirmLabel, danger = false,
}: {
  open: boolean;
  onCancel: () => void;
  onConfirm: () => void;
  title: string;
  body: React.ReactNode;
  confirmLabel: string;
  danger?: boolean;
}) {
  return (
    <Modal open={open} onClose={onCancel} title={title} width={380}
      footer={
        <>
          <Button onClick={onCancel}>Cancel</Button>
          <Button variant={danger ? "danger" : "primary"} onClick={onConfirm}>{confirmLabel}</Button>
        </>
      }
    >
      <div className="text-xs leading-relaxed text-ink-secondary">{body}</div>
    </Modal>
  );
}
