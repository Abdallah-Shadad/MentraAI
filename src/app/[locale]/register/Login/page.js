"use client";
import { useState, useEffect } from "react";
import { useRouter } from "@/lib/i18n/navigation";
//component
import { BackgroundNodes } from "@/components/reusable/BackgroundNodes";
import ErrorState from "@/components/reusable/ErrorState";
//icons
import { ArrowRight, Loader2 } from "lucide-react";
//hooks
import { useLogin } from "@/hooks/useAuth";

export default function LoginPage() {
  const router = useRouter();
  const [formData, setFormData] = useState({
    email: "",
    password: "",
  });
  const { mutate: login, isPending, isError, error, data, reset } = useLogin();

  const handleSubmit = (e) => {
    e.preventDefault();
    login({ ...formData });
  };
  useEffect(() => {
    if (data?.success) {
      router.push("/student");
    }
  }, [data, router]);

  return (
    <div className="min-h-screen w-full flex items-center justify-center bg-background p-4 font-sans relative overflow-hidden">
      <BackgroundNodes />
      {/* Background Glows (Optimized with v4 filters */}
      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        <div className="absolute -top-24 -left-24 w-96 h-96 bg-surface opacity-10 blur-[120px] rounded-full"></div>
        <div className="absolute -bottom-24 -right-24 w-96 h-96 bg-secondary opacity-10 blur-[120px] rounded-full"></div>
      </div>

      <div className="relative z-10 w-full max-w-5xl grid lg:grid-cols-2 bg-card/50 backdrop-blur-sm rounded-lg border border-border overflow-hidden shadow-shadow-card">
        {isError && (
          <ErrorState
            message={error?.response?.data?.error?.message}
            close={() => {
              reset();
            }}
          />
        )}
        {/* Left Side: Brand Experience */}
        <div className="hidden lg:flex flex-col justify-center p-12 bg-linear-to-br from-surface-elevated to-bg-surface border-r border-border">
          <div className="flex items-center gap-3 mb-10">
            <div className="w-12 h-12 rounded-full bg-linear-to-br from-primary to-secondary flex items-center justify-center shadow-shadow-neon">
              <span className="text-foreground font-bold text-2xl">M</span>
            </div>
            <span className="primary-gradient font-bold text-2xl">
              MentraAI
            </span>
          </div>

          <h2 className="text-5xl font-black text-foreground mb-6 leading-[1.1]">
            Code your <br />
            <span className="text-transparent bg-clip-text bg-linear-to-r from-primary to-secondary">
              future
            </span>{" "}
            today.
          </h2>

          <p className="text-foreground-muted text-lg mb-10 max-w-md">
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
                className="flex items-center gap-3 text-foreground-secondary font-medium"
              >
                <div className="w-1.5 h-1.5 rounded-full shadow-[0_0_10px_currentcolor] bg-text-foreground"></div>
                {item.label}
              </div>
            ))}
          </div>
        </div>
        {/* Right Side: Login Form */}
        <div className="p-8 md:p-14 flex flex-col justify-center">
          <div className="mb-10 text-center lg:text-left">
            <h3 className="text-3xl font-bold text-foreground mb-2">
              Welcome Back
            </h3>
            <p className="text-foreground-muted">
              New here?{" "}
              <a
                href="#"
                className="text-foreground-light hover:text-foreground underline underline-offset-4 transition-colors"
              >
                Create an account
              </a>
            </p>
          </div>

          <form className="group flex flex-col gap-6">
            <div>
              <label className="text-sm font-semibold text-foreground-secondary tracking-wide mb-2 block">
                EMAIL ADDRESS
              </label>
              <input
                type="email"
                value={formData.email}
                onChange={(e) =>
                  setFormData({ ...formData, email: e.target.value })
                }
                placeholder="developer@mentra.ai"
                className="w-full bg-bg-surface border border-border rounded-md px-4 py-3.5 text-foreground outline-none ring-ring/0 focus:ring-2 focus:ring-ring focus:border-transparent transition-all duration-200 placeholder:text-foreground-muted/50"
              />
            </div>

            <div className="space-y-2">
              <div className="flex justify-between items-center">
                <label className="text-sm font-semibold text-foreground-secondary ">
                  PASSWORD
                </label>
                <a
                  href="#"
                  className="text-xs text-foreground-muted hover:text-foreground-light transition-colors"
                >
                  Forgot Password?
                </a>
              </div>
              <input
                type="password"
                value={formData.password}
                onChange={(e) =>
                  setFormData({ ...formData, password: e.target.value })
                }
                placeholder="••••••••"
                className="w-full bg-bg-surface border border-border rounded-md px-4 py-3.5 text-foreground outline-none ring-ring/0 focus:ring-2 focus:ring-ring focus:border-transparent transition-all duration-200 placeholder:text-foreground-muted/50"
              />
            </div>

            <button
              type="submit"
              disabled={isPending}
              onClick={handleSubmit}
              className="w-full flex items-center justify-center gap-2 py-4 bg-primary/20 border border-primary hover:bg-primary text-foreground font-bold rounded-md shadow-shadow-neon transition-all active:scale-[0.99] cursor-pointer"
            >
              {isPending ? (
                <div className="flex items-center gap-2">
                  <Loader2 className="animate-spin" />
                  <span>Logging in...</span>
                </div>
              ) : (
                <div className="flex items-center gap-2">
                  <span>Login to Dashboard</span>
                  <span>
                    <ArrowRight />
                  </span>
                </div>
              )}
            </button>

            <div className="relative py-4">
              <div className="absolute inset-0 flex items-center">
                <div className="w-full border-t border-border"></div>
              </div>
              <div className="relative flex justify-center text-xs uppercase font-bold tracking-widest text-foreground-muted">
                <span className="bg-card px-4">Authorized Access Only</span>
              </div>
            </div>
          </form>

          <p className="mt-10 text-center text-foreground-muted text-xs">
            By logging in, you agree to our{" "}
            <a href="#" className="hover:text-foreground underline">
              Terms of Service
            </a>
            .
          </p>
        </div>
      </div>
    </div>
  );
}
