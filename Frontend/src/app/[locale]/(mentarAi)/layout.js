import Navbar from "@/components/reusable/navbar";
import Footer from "@/components/reusable/footer";

export const metadata = {
  title: "User",
  description: "User",
  icons: {
    icon: "/Logo/mentra-app-icon.svg",
    shortcut: "/Logo/mentra-app-icon.svg",
    apple: "/Logo/mentra-app-icon.svg",
  },
};

export default async function UserLayout({ children }) {
  return (
    <main>
      <Navbar />
      {children}
      <Footer />
    </main>
  );
}
