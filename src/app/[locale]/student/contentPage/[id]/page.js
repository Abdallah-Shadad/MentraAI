"use client";

import { useState } from "react";
import { useParams } from "next/navigation";
import LessonSidebar from "../_sections/LessonSidebar";
import LessonContent from "../_sections/LessonContent";
//hooks
import {
  useGetStageResources1,
  useGetStageResources2,
} from "@/hooks/useResource";

export default function LessonPage() {
  const params = useParams();
  const id = params.id;
  const [openSidebar, setOpenSidebar] = useState(false);
  const [numoflesson, setNumoflesson] = useState(0);

  // get resources
  const { data: resources1 } = useGetStageResources1(id);
  const { data: resources2 } = useGetStageResources2(id);
  const resources =
    resources1?.data?.resources || resources2?.data?.resources || [];

  console.log("resources", resources);

  return (
    <div className="min-h-screen w-full flex bg-background text-foreground font-sans">
      <LessonSidebar
        open={openSidebar}
        lessons={resources?.videos || []}
        setNumoflesson={setNumoflesson}
      />
      <LessonContent
        setOpenSidebar={setOpenSidebar}
        video={resources?.videos?.[numoflesson] || {}}
        article={resources?.articles?.[numoflesson] || {}}
      />
      {openSidebar && (
        <div
          className="lg:hidden fixed inset-0 bg-black/20 z-10"
          onClick={() => setOpenSidebar(false)}
        ></div>
      )}
    </div>
  );
}
