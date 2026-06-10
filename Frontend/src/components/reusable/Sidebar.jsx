"use client";
import { useState } from "react";
import ThemeToggle from "@/components/reusable/ThemeToggle";
import { Home, Menu, X, Map } from "lucide-react";
import { Link } from "@/lib/i18n/navigation";
import { usePathname } from "@/lib/i18n/navigation";

export default function Sidebar() {
  const [open, setOpen] = useState(false);
  const pathname = usePathname();

  const links = [
    { name: "Home", icon: Home, href: "/student" },
    { name: "Roadmap", icon: Map, href: "/student/roadmap" },
  ];

  return (
    <>
      {/* Mobile Top Bar */}
      <div className="lg:hidden flex items-center justify-between p-4 border-b border-border bg-card">
        <h1 className="font-bold text-foreground">My App</h1>

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
          bg-card border-r border-border
          transform transition-transform duration-300 z-50
          ${open ? "translate-x-0" : "-translate-x-full lg:translate-x-0"}
        `}
      >
        {/* Header */}
        <div className="p-4 border-b border-border flex items-center justify-between gap-4">
          <h1 className="font-bold text-foreground">Dashboard</h1>
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
      </aside>
    </>
  );
}
