//icons
import {
  Map,
  ClipboardCheck,
  Brain,
  GraduationCap,
  Compass,
  Sparkles,
  Users,
} from "lucide-react";

const features = [
  {
    icon: Map,
    title: "Structured Learning Paths",
    description:
      "Step-by-step learning journeys designed to take you from beginner to mastery with clarity and focus.",
    color: "secondary",
  },
  {
    icon: ClipboardCheck,
    title: "Mastery-Based Progress",
    description:
      "Advance only when you truly understand — continuous assessments ensure deep, lasting knowledge.",
    color: "primary",
  },
  {
    icon: Brain,
    title: "AI-Powered Mentor",
    description:
      "Your personal AI mentor that tracks your progress, answers your questions, and guides you 24/7.",
    color: "secondary",
  },
  {
    icon: GraduationCap,
    title: "Verified Certificate",
    description:
      "Earn a credible certificate that proves your skills and strengthens your professional profile.",
    color: "primary",
  },
  {
    icon: Compass,
    title: "Career Acceleration",
    description:
      "Get expert guidance and a clear roadmap to confidently move toward real job opportunities.",
    color: "secondary",
  },
  {
    icon: Users,
    title: "Community & Support",
    description:
      "Join a driven community of learners, get support, share knowledge, and grow together.",
    color: "primary",
  },
];
export default function FeaturesSection() {
  return (
    <section id="features" className="section-padding relative overflow-hidden">
      <div className="main-container">
        {/* Title */}
        <div className="container-custom">
          <div className="text-center mb-12">
            <div className="inline-flex items-center gap-2 px-3 py-1.5 rounded-full bg-secondary/15 text-secondary border border-secondary/50 text-sm font-medium mb-4">
              <Sparkles size={14} />
              <span>Platform Features</span>
            </div>
            <h2 className="text-2xl sm:text-3xl md:text-4xl font-bold text-text-primary mb-4">
              Everything You Need in{" "}
              <span className="primary-gradient">One Platform</span>
            </h2>
            <p className="text-base md:text-lg text-text-muted">
              Smart tools powered by artificial intelligence for a successful
              learning journey
            </p>
          </div>

          {/* Features Grid */}
          <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-6">
            {features.map((feature, index) => (
              <div
                key={index}
                className={`group relative ${index === 4 ? "lg:col-start-2" : ""}`}
              >
                <div className="p-6 h-full relative bg-card/50 backdrop-blur-sm border border-border rounded-2xl hover:border-text-muted/30 transition-all duration-300">
                  {/* Top gradient accent line on hover */}
                  <div
                    className={`absolute top-0 left-3 right-3 h-1 rounded-2xl opacity-0 group-hover:opacity-100 transition-opacity duration-300 ${
                      feature.color === "secondary"
                        ? "bg-linear-to-r from-secondary via-secondary/70 to-secondary/30"
                        : "bg-linear-to-r from-primary via-primary/70 to-primary/30"
                    }`}
                  />

                  <div className="flex items-start gap-4">
                    <div
                      className={`relative w-14 h-14 shrink-0 rounded-xl flex items-center justify-center transition-all duration-300 group-hover:scale-110 ${
                        feature.color === "secondary"
                          ? "bg-text-muted/10 group-hover:bg-text-muted/20"
                          : "bg-primary/10 group-hover:bg-primary/20"
                      }`}
                    >
                      <feature.icon
                        className={`w-6 h-6 ${
                          feature.color === "secondary"
                            ? "text-text-muted"
                            : "text-primary"
                        }`}
                      />
                    </div>

                    <div>
                      <h3 className="font-bold text-text-primary mb-2 text-lg">
                        {feature.title}
                      </h3>
                      <p className="text-sm text-text-muted leading-relaxed">
                        {feature.description}
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </section>
  );
}
