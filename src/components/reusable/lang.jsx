"use client";
import { useLocale } from "next-intl";
import { useRouter, usePathname } from "@/lib/i18n/navigation";
export default function LanguageSwitcher() {
  const locale = useLocale();
  const router = useRouter();
  const pathname = usePathname();

  function switchLocale(newLocale) {
    if (newLocale !== locale) {
      router.replace(pathname, { locale: newLocale });
      router.refresh();
    }
  }

  return (
    <select value={locale} onChange={(e) => switchLocale(e.target.value)}>
      <option value="en">English</option>
      <option value="ar">العربية</option>
    </select>
  );
}
