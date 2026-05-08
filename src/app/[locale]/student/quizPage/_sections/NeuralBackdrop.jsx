"use client";

export default function NeuralBackdrop() {
  return (
    <div className="pointer-events-none absolute inset-0 overflow-hidden">
      <div className="absolute -top-32 -left-24 size-[480px] rounded-full bg-primary/20 blur-[120px]" />
      <div className="absolute top-1/3 -right-32 size-[420px] rounded-full bg-secondary/15 blur-[120px]" />
      <div className="absolute bottom-0 left-1/3 size-[360px] rounded-full bg-accent/15 blur-[120px]" />
      <svg
        className="absolute inset-0 w-full h-full opacity-[0.06]"
        xmlns="http://www.w3.org/2000/svg"
      >
        <defs>
          <pattern
            id="grid"
            width="48"
            height="48"
            patternUnits="userSpaceOnUse"
          >
            <path
              d="M 48 0 L 0 0 0 48"
              fill="none"
              stroke="white"
              strokeWidth="0.5"
            />
          </pattern>
        </defs>
        <rect width="100%" height="100%" fill="url(#grid)" />
      </svg>
    </div>
  );
}
