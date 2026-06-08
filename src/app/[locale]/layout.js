import { NextIntlClientProvider } from "next-intl";
import { Inter, Roboto, Playfair_Display } from "next/font/google";
import MyClient from "@/components/reusable/MyClient";
import "@/_style/globals.css";
const inter = Inter({
  subsets: ["latin", "latin-ext"],
  weight: ["400", "500", "600", "700"],
  variable: "--font-inter",
});

const roboto = Roboto({
  subsets: ["latin", "latin-ext"],
  weight: ["400", "500", "700"],
  variable: "--font-roboto",
});

const playfair = Playfair_Display({
  subsets: ["latin"], // دعم الإنجليزية
  weight: ["400", "500", "700"], // الأوزان اللي هتحتاجها
  variable: "--font-display", // لو هتستخدمه كـ CSS variable
});

export const metadata = {
  title: "MentarAi",
  description: "MentarAi - Your AI Study Companion",
};

export default async function UserLayout({ children }) {
  return (
    <html lang="en" dir="ltr">
      <body
        className={` ${inter.variable} ${roboto.variable} ${playfair.variable} antialiased`}
      >
        <MyClient>
          <NextIntlClientProvider>{children}</NextIntlClientProvider>
        </MyClient>
      </body>
    </html>
  );
}
