import Sidebar from "@/components/reusable/Sidebar";

export default function StudentLayout({ children }) {
  return (
    <>
      <Sidebar />
      <main className="lg:ml-56">{children}</main>
    </>
  );
}
