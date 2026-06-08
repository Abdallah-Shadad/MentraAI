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
import { useGetCurrentUser } from "@/hooks/useAuth";

const Navbar = () => {
  const [isOpen, setIsOpen] = useState(false);
  const t = useTranslations("Navbar");
  const user = useGetCurrentUser();

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
            <div className="relative flex h-9 w-9 items-center justify-center rounded-xl bg-linear-to-br from-primary to-secondary transition-shadow group-hover:shadow-neon">
              <Brain className="h-5 w-5 text-foreground-text-foreground" />
            </div>
            <span className="primary-gradient font-bold text-xl">MentraAi</span>
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
          {!user?.isSuccess ? (
            <Link href="/register/Login">
              <Button
                variant="ghost"
                className="rounded-full border border-border px-6 text-foreground-muted hover:text-foreground cursor-pointer"
              >
                Login
              </Button>
            </Link>
          ) : (
            <Button
              onClick={() => router.push("/student")}
              className="rounded-full gradient-cta px-6 hover:opacity-90 hover:scale-105 cursor-pointer"
            >
              Dashboard
            </Button>
          )}

          <Link href="/register/SignUp">
            <Button className="rounded-full gradient-cta px-6 hover:opacity-90 hover:scale-105 cursor-pointer">
              Sign up
            </Button>
          </Link>
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
              <Link href="/register/Login">
                <Button
                  variant="ghost"
                  className="w-full rounded-full border border-border text-foreground-muted hover:text-foreground"
                >
                  Login
                </Button>
              </Link>
              <Link href="/register/SignUp">
                <Button className="w-full rounded-full gradient-cta text-foreground hover:bg-gradient-cta cursor-pointer">
                  Sign up
                </Button>
              </Link>
            </div>
          </div>
        </div>
      )}
    </nav>
  );
};

export default Navbar;
