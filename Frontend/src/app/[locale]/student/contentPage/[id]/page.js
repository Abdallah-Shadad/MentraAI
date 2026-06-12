"use client";

import { useState } from "react";
import { useParams } from "next/navigation";
import LessonSidebar from "../_sections/LessonSidebar";
import LessonContent from "../_sections/LessonContent";
//hooks
import { useStageResources } from "@/hooks/useResource";
import { Loader2, BookOpenText } from "lucide-react";

export default function LessonPage() {
  const params = useParams();
  const id = params.id;
  const [openSidebar, setOpenSidebar] = useState(false);
  const [numoflesson, setNumoflesson] = useState(0);

  // get resources using single unified fallback hook
  const { data: resourcesResponse, isLoading, isError, error } = useStageResources(id);
  const resources = resourcesResponse?.data?.resources || resourcesResponse?.resources || {};

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

  if (isError) {
    return (
      <div className="min-h-screen w-full flex items-center justify-center bg-background p-6">
        <div className="max-w-md text-center space-y-4">
          <h2 className="text-xl font-bold text-destructive">Failed to Load Resources</h2>
          <p className="text-foreground-muted text-sm">
            {error?.response?.data?.error?.message || error?.message || "An error occurred while loading learning resources."}
          </p>
          <button 
            onClick={() => window.location.reload()}
            className="px-4 py-2 bg-primary text-white rounded-lg font-semibold hover:bg-primary/90 transition cursor-pointer"
          >
            Retry Loading
          </button>
        </div>
      </div>
    );
  }

  console.log(resources);

  return (
    <div className="h-screen w-full flex overflow-hidden bg-background text-foreground font-sans">
      <LessonSidebar
        open={openSidebar}
        lessons={resources?.videos || []}
        setNumoflesson={setNumoflesson}
        stageProgressId={id}
      />
      <LessonContent
        setOpenSidebar={setOpenSidebar}
        video={resources?.videos?.[numoflesson] || {}}
        article={resources?.articles?.[numoflesson] || {}}
        stageProgressId={id}
        isLastLesson={numoflesson === ((resources?.videos?.length || 1) - 1)}
        onNextLesson={() => setNumoflesson((prev) => prev + 1)}
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
