"use client";

import { useEffect, useState } from "react";
import { Moon, Sun } from "lucide-react";

export default function ThemeToggle() {
  const [theme, setTheme] = useState("light");

  // تحميل الثيم عند فتح الصفحة
  useEffect(() => {
    const savedTheme = localStorage.getItem("theme");

    if (savedTheme === "dark") {
      document.documentElement.classList.add("dark");
      setTheme("dark");
    } else {
      document.documentElement.classList.remove("dark");
      setTheme("light");
    }
  }, []);

  // التبديل بين light / dark
  const toggleTheme = () => {
    const isDark = document.documentElement.classList.toggle("dark");

    const newTheme = isDark ? "dark" : "light";
    localStorage.setItem("theme", newTheme);
    setTheme(newTheme);
  };

  return (
    <button
      onClick={toggleTheme}
      className="px-4 py-2 rounded-full border border-border bg-card text-foreground transition-all duration-300 hover:scale-105 flex items-center gap-2 cursor-pointer"
    >
      {theme === "dark" ? (
        <>
          <Moon className="h-5 w-5" /> Dark
        </>
      ) : (
        <>
          <Sun className="h-5 w-5" /> Light
        </>
      )}
    </button>
  );
}
