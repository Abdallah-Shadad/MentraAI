"use client";
//icons
import {
  Target,
  Route,
  CheckCircle,
  Award,
  Sparkles,
  ArrowRight,
} from "lucide-react";
import { motion } from "framer-motion";

const steps = [
  {
    icon: Target,
    number: "01",
    title: "Choose Your Goal",
    description: "Set your goal or let AI suggest the best path for you",
    color: "primary",
  },
  {
    icon: Route,
    number: "02",
    title: "Follow an Organized Path",
    description:
      "A learning path built on your current level that evolves with you",
    color: "secondary",
  },
  {
    icon: CheckCircle,
    number: "03",
    title: "Test Your Understanding",
    description:
      "Interactive assessments ensure comprehension before moving on",
    color: "primary",
  },
  {
    icon: Award,
    number: "04",
    title: "Get Certified",
    description:
      "Earn a recognized certificate and career guidance for your journey",
    color: "secondary",
  },
];

export default function StepsSection() {
  return (
    <section
      id="how-it-works"
      className="section-padding relative overflow-hidden"
    >
      <div className="main-container">
        <div className="container-custom">
          <div className="text-center mb-16">
            <div className="inline-flex items-center gap-2 px-3 py-1.5 rounded-full bg-secondary/15 text-secondary border border-secondary/50 text-sm font-medium mb-4">
              <Sparkles size={14} />
              <span>How It Works</span>
            </div>
            <h2 className="text-2xl sm:text-3xl md:text-4xl font-bold text-foreground mb-4">
              Four Steps to <span className="primary-gradient">Success</span>
            </h2>
            <p className="text-base md:text-lg text-foreground-muted">
              An organized journey transforming you from confused learner to
              confident professional
            </p>
          </div>

          <div className="relative">
            {/* Connection line - desktop only */}
            <div className="hidden lg:block absolute top-1/2 left-12 right-12 h-0.5 -translate-y-1/2 pointer-events-none">
              <div className="h-full bg-linear-to-r from-primary via-secondary to-primary rounded-full opacity-30" />
              {/* Animated dot on the line */}
              <motion.div
                initial={{ left: "0%" }}
                animate={{ left: "100%" }}
                transition={{ duration: 2, repeat: Infinity, ease: "linear" }}
                className="absolute top-1/2 -translate-y-1/2 w-3 h-3 rounded-full bg-secondary shadow-[0_0_12px_4px] shadow-secondary/50"
              ></motion.div>
            </div>

            <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-8">
              {steps.map((step, index) => (
                <motion.div
                  key={index}
                  className="relative group pb-4"
                  initial={{ opacity: 0 }}
                  whileInView={{ opacity: 1 }}
                  transition={{ duration: 1, delay: index * 0.2 }}
                  viewport={{ amount: 0.1, once: true }}
                >
                  {/* Step card */}
                  <div className="p-6 text-center relative z-10 card/50 backdrop-blur-sm border border-border rounded-2xl h-full hover:border-secondary/30 transition-all duration-300">
                    {/* Number badge */}
                    <div
                      className={`absolute -top-3 left-4 px-3 py-1 rounded-full text-sm font-bold text-foreground/80 ${
                        step.color === "primary"
                          ? "bg-linear-to-r from-primary to-primary/80 text-foreground-text-foreground shadow-[0_0_15px] shadow-primary/30"
                          : "bg-linear-to-r from-secondary to-secondary/80 text-secondary-text-foreground shadow-[0_0_15px] shadow-secondary/30"
                      }`}
                    >
                      {step.number}
                    </div>

                    <div
                      className={`w-16 h-16 mx-auto mb-4 mt-2 rounded-2xl flex items-center justify-center transition-all group-hover:scale-110 ${
                        step.color === "primary"
                          ? "bg-surface/10 group-hover:bg-surface/20"
                          : "bg-secondary/10 group-hover:bg-secondary/20"
                      }`}
                    >
                      <step.icon
                        className={`w-8 h-8 ${
                          step.color === "primary"
                            ? "text-foreground"
                            : "text-secondary"
                        }`}
                      />
                    </div>

                    <h3 className="font-bold text-lg text-foreground mb-2">
                      {step.title}
                    </h3>
                    <p className="text-sm md:text-base text-foreground-muted leading-relaxed">
                      {step.description}
                    </p>
                  </div>

                  {/* Arrow connector for mobile / tablet */}
                  {index < steps.length - 1 && (
                    <div className="md:hidden flex justify-center py-4">
                      <ArrowRight className="w-6 h-6 text-secondary/40 rotate-90" />
                    </div>
                  )}
                </motion.div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
