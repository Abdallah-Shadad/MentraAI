"use client";

import { useState } from "react";
import { Menu, X, Brain } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Link } from "@/lib/i18n/navigation";
const Navbar = () => {
  const [isOpen, setIsOpen] = useState(false);

  const navLinks = [
    { name: "Features", href: "#features" },
    { name: "How It Works", href: "#how-it-works" },
    { name: "About Us", href: "#about" },
  ];

  return (
    <nav className="fixed inset-x-0 top-0 z-50 bg-background/60 backdrop-blur-xl">
      <div className="flex h-16 items-center justify-between main-container">
        {/* Logo */}
        <Link href="/" className="group flex items-center gap-2">
          <div className="relative flex h-9 w-9 items-center justify-center rounded-xl bg-linear-to-br from-primary to-secondary transition-shadow group-hover:shadow-neon">
            <Brain className="h-5 w-5 text-primary-foreground" />
          </div>
          <span className="primary-gradient font-bold text-xl">MentraAi</span>
        </Link>

        {/* Desktop Navigation */}
        <div className="hidden md:flex items-center gap-8">
          {navLinks.map((link) => (
            <a
              key={link.name}
              href={link.href}
              className="text-sm font-medium text-text-muted transition-colors hover:text-text-foreground"
            >
              {link.name}
            </a>
          ))}
        </div>

        {/* Desktop CTA */}
        <div className="hidden md:flex items-center gap-3">
          <Link href="/register/Login">
            <Button
              variant="ghost"
              className="rounded-full border border-border px-6 text-text-muted hover:text-text-foreground cursor-pointer"
            >
              Login
            </Button>
          </Link>

          <Link href="/register/SignUp">
            <Button className="rounded-full gradient-cta px-6 hover:opacity-90 hover:scale-105 cursor-pointer">
              Sign up
            </Button>
          </Link>
        </div>

        {/* Mobile Toggle */}
        <button
          className="md:hidden rounded-md p-2 text-text-primary"
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
                className="block py-2 font-medium text-text-muted transition-colors hover:text-text-foreground"
              >
                {link.name}
              </a>
            ))}

            <div className="flex flex-col gap-2 border-t border-border pt-4">
              <Link href="/register/Login">
                <Button
                  variant="ghost"
                  className="w-full rounded-full border border-border text-text-muted hover:text-text-foreground"
                >
                  Login
                </Button>
              </Link>
              <Link href="/register/SignUp">
                <Button className="w-full rounded-full gradient-cta text-text-foreground hover:bg-gradient-cta cursor-pointer">
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
