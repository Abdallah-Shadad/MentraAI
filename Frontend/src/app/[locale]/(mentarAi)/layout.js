import Navbar from "@/components/reusable/navbar";
import Footer from "@/components/reusable/footer";

export const metadata = {
  title: "User",
  description: "User",
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
