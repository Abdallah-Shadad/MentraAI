"use client";

import { useState, useEffect } from "react";
import { useTranslations } from "next-intl";
import { Link, useRouter } from "@/lib/i18n/navigation";

//components
import { Button } from "@/components/ui/button";
import ThemeToggle from "./ThemeToggle";
//icons
import { Menu, X, Brain } from "lucide-react";
//hooks
import { useGetCurrentUser, useLogout } from "@/hooks/useAuth";

const Navbar = () => {
  const [isOpen, setIsOpen] = useState(false);
  const t = useTranslations("Navbar");
  const user = useGetCurrentUser();
  const { mutate: performLogout, isPending: isLoggingOut } = useLogout();

  const router = useRouter();

  const navLinks = [
    { name: "Real Problems", href: "#problem" },
    { name: "Features", href: "#features" },
    { name: "How It Works", href: "#how-it-works" },
  ];

  return (
    <nav className="fixed inset-x-0 top-0 z-50 bg-background/60 backdrop-blur-xl">
      <div className="flex h-16 items-center justify-between main-container">
        {/* Logo */}
        <div className="flex items-center gap-4">
          <Link href="/" className="group flex items-center gap-2">
            <img src="/Logo/mentra-logo-light.svg" alt="MentraAI Logo" className="h-9 w-auto object-contain dark:hidden" />
            <img src="/Logo/mentra-logo-dark.svg" alt="MentraAI Logo" className="h-9 w-auto object-contain hidden dark:block" />
          </Link>
          <ThemeToggle />
        </div>

        {/* Desktop Navigation */}
        <div className="hidden md:flex items-center gap-8">
          {navLinks.map((link) => (
            <a
              key={link.name}
              href={link.href}
              className="text-sm font-medium text-foreground-muted transition-colors hover:text-foreground"
            >
              {link.name}
            </a>
          ))}
        </div>

        {/* Desktop CTA */}
        <div className="hidden md:flex items-center gap-3">
          {!user?.data ? (
            <>
              <Link href="/login">
                <Button
                  variant="ghost"
                  className="rounded-full border border-border px-6 text-foreground-muted hover:text-foreground cursor-pointer"
                >
                  Login
                </Button>
              </Link>
              <Link href="/signup">
                <Button className="rounded-full gradient-cta px-6 hover:opacity-90 hover:scale-105 cursor-pointer">
                  Sign up
                </Button>
              </Link>
            </>
          ) : (
            <>
              <Link href="/student/homepage">
                <Button
                  className="rounded-full gradient-cta px-6 hover:opacity-90 hover:scale-105 cursor-pointer"
                >
                  Dashboard
                </Button>
              </Link>
              <Button
                onClick={() => performLogout()}
                disabled={isLoggingOut}
                className="rounded-full bg-red-500/10 border border-red-500/20 text-red-500 px-6 hover:bg-red-500 hover:text-white transition cursor-pointer disabled:opacity-50"
              >
                Logout
              </Button>
            </>
          )}
        </div>

        {/* Mobile Toggle */}
        <button
          className="md:hidden rounded-md p-2 text-foreground"
          onClick={() => setIsOpen((prev) => !prev)}
        >
          {isOpen ? <X size={24} /> : <Menu size={24} />}
        </button>
      </div>

      {/* Mobile Menu */}
      {isOpen && (
        <div className="md:hidden border-t border-border bg-background/95 backdrop-blur-xl">
          <div className="space-y-3 px-4 py-4">
            {navLinks.map((link) => (
              <a
                key={link.name}
                href={link.href}
                onClick={() => setIsOpen(false)}
                className="block py-2 font-medium text-foreground-muted transition-colors hover:text-foreground"
              >
                {link.name}
              </a>
            ))}

            <div className="flex flex-col gap-2 border-t border-border pt-4">
              {!user?.data ? (
                <>
                  <Link href="/login" onClick={() => setIsOpen(false)}>
                    <Button
                      variant="ghost"
                      className="w-full rounded-full border border-border text-foreground-muted hover:text-foreground"
                    >
                      Login
                    </Button>
                  </Link>
                  <Link href="/signup" onClick={() => setIsOpen(false)}>
                    <Button className="w-full rounded-full gradient-cta text-foreground hover:bg-gradient-cta cursor-pointer">
                      Sign up
                    </Button>
                  </Link>
                </>
              ) : (
                <>
                  <Link href="/student/homepage" onClick={() => setIsOpen(false)}>
                    <Button className="w-full rounded-full gradient-cta text-foreground hover:bg-gradient-cta cursor-pointer">
                      Dashboard
                    </Button>
                  </Link>
                  <Button
                    onClick={() => {
                      performLogout();
                      setIsOpen(false);
                    }}
                    disabled={isLoggingOut}
                    className="w-full rounded-full bg-red-500/10 border border-red-500/20 text-red-500 hover:bg-red-500 hover:text-white transition cursor-pointer disabled:opacity-50"
                  >
                    Logout
                  </Button>
                </>
              )}
            </div>
          </div>
        </div>
      )}
    </nav>
  );
};

export default Navbar;
