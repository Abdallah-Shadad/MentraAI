"use client";
import { useState } from "react";
import ThemeToggle from "@/components/reusable/ThemeToggle";
import { Home, Menu, X, Map, LayoutDashboard, LogOut, Loader2, Sparkles } from "lucide-react";
import { Link } from "@/lib/i18n/navigation";
import { usePathname } from "@/lib/i18n/navigation";
import { useLogout } from "@/hooks/useAuth";

export default function Sidebar() {
  const [open, setOpen] = useState(false);
  const pathname = usePathname();
  const { mutate: performLogout, isPending: isLoggingOut } = useLogout();

  const links = [
    { name: "Home", icon: Home, href: "/student/homepage" },
    { name: "Dashboard", icon: LayoutDashboard, href: "/student/dashboard" },
    { name: "Roadmap", icon: Map, href: "/student/roadmap" },
  ];

  return (
    <>
      {/* Mobile Top Bar */}
      <div className="lg:hidden flex items-center justify-between p-4 border-b border-border bg-card">
        <Link href="/student/homepage" className="flex items-center gap-2">
          <img src="/Logo/mentra-logo-light.svg" alt="MentraAI Logo" className="h-7 w-auto object-contain dark:hidden" />
          <img src="/Logo/mentra-logo-dark.svg" alt="MentraAI Logo" className="h-7 w-auto object-contain hidden dark:block" />
        </Link>

        <button onClick={() => setOpen(!open)} className="text-foreground">
          {open ? <X /> : <Menu />}
        </button>
      </div>

      {/* Overlay for mobile */}
      {open && (
        <div
          className="fixed inset-0 bg-black/40 lg:hidden "
          onClick={() => setOpen(false)}
        />
      )}

      {/* Sidebar */}
      <aside
        className={`
          fixed top-0 left-0 min-h-screen w-56 shrink-0
          bg-card border-r border-border flex flex-col justify-between z-50
          transform transition-transform duration-300
          ${open ? "translate-x-0" : "-translate-x-full lg:translate-x-0"}
        `}
      >
        <div>
          {/* Header — Logo */}
          <div className="p-4 border-b border-border flex items-center justify-between gap-4">
            <Link href="/student/homepage" className="flex items-center gap-2 min-w-0">
              <img src="/Logo/mentra-logo-light.svg" alt="MentraAI Logo" className="h-8 w-auto object-contain dark:hidden" />
              <img src="/Logo/mentra-logo-dark.svg" alt="MentraAI Logo" className="h-8 w-auto object-contain hidden dark:block" />
            </Link>
            <ThemeToggle />
          </div>


          {/* Links */}
          <nav className="p-4 space-y-2">
            {links.map((link, i) => {
              const Icon = link.icon;

              return (
                <Link
                  key={i}
                  href={link.href}
                  className={`flex items-center gap-2 p-2 rounded-lg text-foreground transition relative ${
                    pathname === link.href
                      ? "bg-primary/60 text-primary-foreground"
                      : "text-foreground hover:bg-primary/20"
                  }`}
                >
                  <Icon size={18} />
                  {link.name}
                </Link>
              );
            })}
          </nav>
        </div>

        {/* Footer with Logout */}
        <div className="p-4 border-t border-border mt-auto">
          <button
            onClick={() => performLogout()}
            disabled={isLoggingOut}
            className="flex items-center gap-2 w-full p-2.5 rounded-lg text-red-500 hover:bg-red-500/10 transition font-semibold cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isLoggingOut ? (
              <Loader2 className="w-[18px] h-[18px] animate-spin" />
            ) : (
              <LogOut size={18} />
            )}
            <span>Logout</span>
          </button>
        </div>
      </aside>
    </>
  );
}
