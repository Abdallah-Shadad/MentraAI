import { BackgroundNodes } from "../../(mentarAi)/homepage/_components/BackgroundNodes";
import { ArrowRight } from "lucide-react";
import Link from "next/link";

export default function LoginPage() {
  return (
    <div className="min-h-screen w-full flex items-center justify-center bg-background p-4 font-sans selection:bg-primary/30 relative overflow-hidden">
      <BackgroundNodes />
      {/* Background Glows (Optimized with v4 filters */}
      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        <div className="absolute -top-24 -left-24 w-96 h-96 bg-primary opacity-10 blur-[120px] rounded-full"></div>
        <div className="absolute -bottom-24 -right-24 w-96 h-96 bg-secondary opacity-10 blur-[120px] rounded-full"></div>
      </div>

      <div className="relative z-10 w-full max-w-5xl grid lg:grid-cols-2 bg-bg-card/50 backdrop-blur-sm rounded-lg border border-border overflow-hidden shadow-shadow-card">
        {/* Left Side: Brand Experience */}
        <div className="hidden lg:flex flex-col justify-center p-12 bg-linear-to-br from-bg-tertiary to-bg-primary border-r border-border">
          <div className="flex items-center gap-3 mb-10">
            <div className="w-12 h-12 rounded-full bg-linear-to-br from-primary to-secondary flex items-center justify-center shadow-shadow-neon">
              <span className="text-text-primary font-bold text-2xl">M</span>
            </div>
            <span className="primary-gradient font-bold text-2xl">
              MentraAI
            </span>
          </div>

          <h2 className="text-5xl font-black text-text-primary mb-6 leading-[1.1]">
            Code your <br />
            <span className="text-transparent bg-clip-text bg-linear-to-r from-primary to-secondary">
              future
            </span>{" "}
            today.
          </h2>

          <p className="text-text-muted text-lg mb-10 max-w-md">
            Join thousands of students mastering the art of Frontend development
            with AI guidance.
          </p>

          <div className="space-y-5">
            {[
              { label: "Interactive Roadmap" },
              { label: "Real-time AI Mentoring" },
              { label: "Production Projects" },
            ].map((item, i) => (
              <div
                key={i}
                className="flex items-center gap-3 text-text-secondary font-medium"
              >
                <div className="w-1.5 h-1.5 rounded-full shadow-[0_0_10px_currentcolor] bg-text-primary"></div>
                {item.label}
              </div>
            ))}
          </div>
        </div>

        {/* Right Side: Login Form */}
        <div className="p-8 md:p-14 flex flex-col justify-center">
          <div className="mb-10 text-center lg:text-left">
            <h3 className="text-3xl font-bold text-text-primary mb-2">
              Welcome Back
            </h3>
            <p className="text-text-muted">
              New here?{" "}
              <a
                href="#"
                className="text-primary-light hover:text-primary underline underline-offset-4 transition-colors"
              >
                Create an account
              </a>
            </p>
          </div>

          <form
            className="group space-y-6"
            // onSubmit={(e) => e.preventDefault()}
          >
            <div className="space-y-2">
              <label className="text-sm font-semibold text-text-secondary tracking-wide">
                EMAIL ADDRESS
              </label>
              <input
                type="email"
                placeholder="developer@mentra.ai"
                className="w-full bg-bg-primary border border-border rounded-md px-4 py-3.5 text-text-primary outline-none ring-ring/0 focus:ring-2 focus:ring-ring focus:border-transparent transition-all duration-200 placeholder:text-text-muted/50"
              />
            </div>

            <div className="space-y-2">
              <div className="flex justify-between items-center">
                <label className="text-sm font-semibold text-text-secondary tracking-wide">
                  PASSWORD
                </label>
                <a
                  href="#"
                  className="text-xs text-text-muted hover:text-primary-light transition-colors"
                >
                  Forgot Password?
                </a>
              </div>
              <input
                type="password"
                placeholder="••••••••"
                className="w-full bg-bg-primary border border-border rounded-md px-4 py-3.5 text-text-primary outline-none ring-ring/0 focus:ring-2 focus:ring-ring focus:border-transparent transition-all duration-200"
              />
            </div>

            <Link
              href="/student"
              className="w-full flex items-center justify-center gap-2 py-4 bg-primary hover:bg-primary-dark text-text-foreground font-bold rounded-md shadow-shadow-neon transition-all active:scale-[0.99] cursor-pointer"
            >
              <span>Login to Dashboard</span>
              <span>
                <ArrowRight />
              </span>
            </Link>

            <div className="relative py-4">
              <div className="absolute inset-0 flex items-center">
                <div className="w-full border-t border-border"></div>
              </div>
              <div className="relative flex justify-center text-xs uppercase font-bold tracking-widest text-text-muted">
                <span className="bg-bg-card px-4">Authorized Access Only</span>
              </div>
            </div>
          </form>

          <p className="mt-10 text-center text-text-muted text-xs">
            By logging in, you agree to our{" "}
            <a href="#" className="hover:text-text-primary underline">
              Terms of Service
            </a>
            .
          </p>
        </div>
      </div>
    </div>
  );
}
