//components
import { Button } from "@/components/ui/button";
import Image from "next/image";

//icons
import { Play } from "lucide-react";
import { BackgroundNodes } from "../_components/BackgroundNodes";

export default function HeroSection() {
  return (
    <section className="relative overflow-hidden main-container min-h-screen bg-background">
      <BackgroundNodes />
      <div className="py-24 relative z-10">
        <div className="grid lg:grid-cols-2 gap-12 lg:gap-8 items-center min-h-[calc(100vh-120px)]">
          {/* Content */}
          <div className="text-left">
            <h1 className="mb-6 text-4xl sm:text-5xl md:text-4xl lg:text-6xl font-bold leading-[1.1] text-text-foreground">
              Your Smart Learning Journey Starts Here
              <span className="bg-linear-to-r from-primary to-secondary bg-clip-text text-transparent">
                MentraAi
              </span>{" "}
              Guides You <br /> Step-by-Step
            </h1>

            <p className="mb-10 max-w-xl text-lg md:text-lg text-text-muted">
              Unlock your potential with personalized, AI-driven learning paths
              tailored just for you.
            </p>

            <div className="flex flex-col sm:flex-row gap-4">
              <Button
                size="lg"
                className="rounded-xl bg-primary text-text-primary px-8 py-6 text-lg hover:bg-primary/90 transition"
              >
                Start Your Free Trial
              </Button>

              <Button
                size="lg"
                variant="outline"
                className="flex items-center justify-center rounded-xl border border-border px-8 py-6 text-lg text-text-primary hover:text-primary hover:bg-primary/10 transition"
              >
                <Play size={18} className="mr-2" />
                Watch Demo
              </Button>
            </div>
          </div>

          {/* AI Mentor Image */}
          <div className="relative flex justify-center lg:justify-end">
            <div className="relative w-full max-w-lg">
              {/* Glow effects behind image */}
              <div className="absolute inset-0 flex items-center justify-center -z-10">
                <div className="w-96 h-96 bg-secondary/30 rounded-full blur-[100px] animate-pulse" />
                <div
                  className="absolute w-72 h-72 bg-primary/25 rounded-full blur-[80px] animate-pulse"
                  style={{ animationDelay: "1s" }}
                />
              </div>

              <div className="relative animate-float hover:animate-pulse hover:scale-105 transition-all duration-300 ease-in-out">
                <Image
                  src="/ai-mentor-hero.png"
                  alt="AI Powered Mentor - Your intelligent learning companion"
                  className="w-full h-auto drop-shadow-2xl rounded-3xl"
                  width={500}
                  height={500}
                />
                {/* Glowing border */}
                <div className="absolute inset-0 rounded-3xl bg-linear-to-br from-secondary/20 via-transparent to-primary/20 pointer-events-none" />
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
