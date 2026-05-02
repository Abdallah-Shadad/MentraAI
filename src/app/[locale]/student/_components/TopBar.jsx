import { Home } from "lucide-react";
import { Link } from "@/lib/i18n/navigation";

export default function TopBar() {
  return (
    <div className="flex flex-col gap-6 w-full py-6">
      {/* Hero */}
      <section className="relative overflow-hidden rounded-lg border border-border bg-linear-to-br p-4 md:p-6 from-slate-900 to-slate-800 shadow-xl flex flex-col md:flex-row">
        <div className="absolute -right-16 -top-16 h-64 w-64 rounded-full bg-primary/10 blur-3xl"></div>

        <div className="relative z-10 grid md:grid-cols-2 gap-8 items-center">
          <div className="space-y-6">
            <Link
              href="/"
              className="cursor-pointer mb-4 py-1 flex items-center gap-2 text-text-muted font-semibold hover:text-text-foreground disabled:opacity-50 mr-auto"
            >
              <Home className="w-5 h-5" /> Home
            </Link>

            <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-primary/10 border border-primary/20 text-primary text-xs font-semibold uppercase tracking-wider">
              🚀 Path to Mastery
            </div>

            <h1 className="text-4xl md:text-5xl font-extrabold text-text-primary leading-tight">
              Welcome back, <br />
              <span className="text-transparent bg-clip-text bg-linear-to-r from-primary to-secondary">
                shehab Ahmed
              </span>
            </h1>

            <p className="text-text-muted text-lg max-w-lg leading-relaxed">
              Ready to turn code into art? Your journey continues here.
            </p>
          </div>
        </div>

        <div className="hidden md:block">
          <div className="bg-bg-card border border-border rounded-lg p-6 shadow-2xl rotate-2 hover:rotate-0 transition-transform duration-500">
            {/* Top Bar */}
            <div className="flex gap-2 mb-4 border-b border-border pb-3 items-center">
              <div className="w-3 h-3 rounded-full bg-red-400"></div>
              <div className="w-3 h-3 rounded-full bg-yellow-400"></div>
              <div className="w-3 h-3 rounded-full bg-green-400"></div>

              <span className="text-text-muted text-xs ml-2 font-mono">
                mentor.py
              </span>
            </div>

            {/* Code */}
            <pre className="font-mono text-sm leading-relaxed">
              <code className="text-primary">
                {`success = False
attempt = "failed"

while not success:
    attempt = "trying"
    print(f"Attempt: {attempt}... 💪")

    if attempt == "success":
        success = True

print("Success achieved! 🎉 Keep going 🚀")`}
              </code>
            </pre>
          </div>
        </div>
      </section>
    </div>
  );
}
