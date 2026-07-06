"use client";

const VARIANTS = {
  primary: "border-ink bg-ink text-white hover:bg-ink/85",
  default: "border-line-strong bg-card text-ink hover:border-ink",
  accent: "border-primary bg-primary text-white hover:bg-primary-strong",
  danger: "border-danger bg-danger text-white hover:bg-danger/85",
  dangerOutline: "border-danger-line bg-card text-danger hover:border-danger",
  ghost: "border-transparent bg-transparent text-primary hover:bg-primary-soft",
} as const;

export function Button({
  variant = "default", size = "md", className = "", ...props
}: React.ButtonHTMLAttributes<HTMLButtonElement> & { variant?: keyof typeof VARIANTS; size?: "sm" | "md" }) {
  return (
    <button
      className={`inline-flex items-center gap-1.5 whitespace-nowrap rounded-md border font-semibold transition-colors disabled:cursor-not-allowed disabled:opacity-40 ${
        size === "sm" ? "px-2.5 py-1 text-[11px]" : "px-3 py-1.5 text-xs"
      } ${VARIANTS[variant]} ${className}`}
      type="button"
      {...props}
    />
  );
}
