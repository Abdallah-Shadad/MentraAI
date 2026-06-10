import { Zap, Sparkles, Brain, ArrowRight } from "lucide-react";
import { Button } from "@/components/ui/button";

const CTASection = () => {
  return (
    <section className="section-padding">
      <div className="main-container">
        <div className="relative overflow-hidden rounded-3xl p-8 md:p-12 lg:p-16">
          {/* Gradient background */}
          <div className="absolute inset-0 bg-linear-to-br from-primary/20 to-secondary/20" />
          <div className="absolute inset-0 border border-secondary/20 rounded-3xl" />

          {/* AI decoration */}
          <div className="absolute inset-0 z-0 overflow-hidden">
            <div className="absolute top-0 right-0 w-96 h-96 bg-surface/10 rounded-full blur-[100px] ai-pulse" />
            <div
              className="absolute bottom-0 left-0 w-80 h-80 bg-secondary/10 rounded-full blur-[100px] ai-pulse"
              style={{ animationDelay: "1.5s" }}
            />

            {/* Floating elements */}
            <div className="absolute top-10 left-10 w-20 h-20 border border-secondary/20 rounded-2xl rotate-12 animate-float" />
            <div
              className="absolute bottom-10 right-10 w-16 h-16 border border-primary/20 rounded-full animate-float"
              style={{ animationDelay: "1s" }}
            />

            {/* Neural dots */}
            <div className="absolute inset-0 neural-dots opacity-20" />
          </div>

          <div className="relative z-10 text-center">
            <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-secondary/10 border border-secondary/20 text-secondary text-sm font-medium mb-6">
              <Brain size={16} className="animate-pulse" />
              <span>Start Free with AI</span>
            </div>

            <h2 className="text-2xl sm:text-3xl md:text-4xl lg:text-5xl font-bold text-foreground mb-4">
              Start Your Learning Journey{" "}
              <span className="text-gradient-cyan">Today!</span>
            </h2>

            <p className="text-lg text-foreground-muted mb-8 max-w-2xl mx-auto">
              Join thousands of learners who transformed their random learning
              into an organized journey toward professional success
            </p>

            <div className="flex flex-col sm:flex-row gap-4 justify-center">
              <Button
                size="lg"
                className="gradient-cta text-lg px-8 py-6 gap-2"
              >
                <Zap size={20} />
                Start Free Now
              </Button>
              <Button
                size="lg"
                variant="outline"
                className="text-foreground bg-background border-border text-lg px-8 py-6 gap-2"
              >
                Explore Tracks
                <ArrowRight size={20} />
              </Button>
            </div>

            <div className="flex items-center justify-center gap-6 mt-8 muted-text-foreground">
              <div className="flex items-center gap-2">
                <Sparkles size={16} className="text-secondary" />
                <span className="text-sm text-foreground">
                  No credit card required
                </span>
              </div>
              <div className="flex items-center gap-2">
                <Brain size={16} className="text-secondary" />
                <span className="text-sm text-foreground">Free AI Mentor</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
};

export default CTASection;
