import HeroSection from "./_sections/HeroSection";
import ProblemSection from "./_sections/ProblemSection";
import StepsSection from "./_sections/StepsSection";
import FeaturesSection from "./_sections/FeaturesSection";
import CTASection from "./_sections/CTASection";

export default function HomePage() {
  return (
    <main className="min-h-screen bg-background">
      <HeroSection />
      <ProblemSection />
      <FeaturesSection />
      <StepsSection />
      <CTASection />
      {/* <TestimonialsSection /> */}
    </main>
  );
}
