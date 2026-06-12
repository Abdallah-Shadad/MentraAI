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
import { Loader2, BookOpenText } from "lucide-react";

export default function LessonPage() {
  const params = useParams();
  const id = params.id;
  const [openSidebar, setOpenSidebar] = useState(false);
  const [numoflesson, setNumoflesson] = useState(0);

  // get resources
  const { data: resources1, isLoading: isLoading1 } = useGetStageResources1(id);
  const { data: resources2, isLoading: isLoading2 } = useGetStageResources2(id);
  const isLoading = isLoading1 || isLoading2;
  const resources =
    resources1?.data?.resources || resources2?.data?.resources || {};

  if (isLoading) {
    return (
      <div className="min-h-screen w-full flex items-center justify-center bg-background">
        <div className="flex flex-col items-center gap-4">
          <div className="relative">
            <div className="w-16 h-16 rounded-full bg-primary/10 flex items-center justify-center">
              <BookOpenText className="w-8 h-8 text-primary" />
            </div>
            <Loader2 className="w-6 h-6 text-primary animate-spin absolute -top-1 -right-1" />
          </div>
          <p className="text-foreground-muted text-sm font-medium">Loading resources...</p>
        </div>
      </div>
    );
  }

  console.log(resources);

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
