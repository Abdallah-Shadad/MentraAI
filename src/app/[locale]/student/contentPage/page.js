"use client";

import { useState } from "react";
import LessonSidebar from "./_sections/LessonSidebar";
import LessonContent from "./_sections/LessonContent";
import AIMentorPanel from "./_sections/AIMentorPanel";

export default function LessonPage() {
  const [openSidebar, setOpenSidebar] = useState(false);
  return (
    <div className="min-h-screen w-full flex bg-background text-text-foreground font-sans">
      <LessonSidebar open={openSidebar} />
      <LessonContent setOpenSidebar={setOpenSidebar} />
      <AIMentorPanel />
      {openSidebar && (
        <div
          className="lg:hidden fixed inset-0 bg-black/20 z-10"
          onClick={() => setOpenSidebar(false)}
        ></div>
      )}
    </div>
  );
}
