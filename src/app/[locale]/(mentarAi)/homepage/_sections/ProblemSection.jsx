//icons
import {
  Shuffle,
  HelpCircle,
  XCircle,
  Layers,
  AlertTriangle,
} from "lucide-react";

const problems = [
  {
    icon: Shuffle,
    title: "Random Learning",
    description: "Jumping between topics without a clear plan or defined goal",
  },
  {
    icon: HelpCircle,
    title: "Not Knowing Where to Start",
    description:
      "Too many options causing confusion and delaying your progress",
  },
  {
    icon: XCircle,
    title: "No Understanding Verification",
    description: "Learning without knowing your actual comprehension level",
  },
  {
    icon: Layers,
    title: "Scattered Resources",
    description: "Disconnected materials with no logical organization",
  },
];

export default function ProblemSection() {
  return (
    <section
      id="about"
      className="main-container section-padding relative overflow-hidden py-20"
    >
      {/* Section Title */}
      <div className="text-center mb-12">
        <div className="inline-flex items-center gap-2 px-3 py-1.5 rounded-full bg-destructive/10 border border-destructive/20 text-destructive text-sm font-medium mb-4">
          <AlertTriangle size={14} />
          <span>Self-Learning Challenges</span>
        </div>
        <h2 className="text-2xl sm:text-3xl md:text-4xl font-bold text-text-primary mb-4">
          Struggling with Self-Learning?
        </h2>
        <p className="text-text-muted text-lg max-w-2xl mx-auto">
          Most learners face these challenges — MentraAi was designed
          specifically to solve them
        </p>
      </div>

      <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-6">
        {problems.map((problem, index) => (
          <div
            key={index}
            className="group relative p-6 rounded-2xl bg-card/50 backdrop-blur-sm border border-border hover:border-destructive/30 transition-all duration-300"
          >
            {/* Hover glow overlay */}
            <div className="absolute inset-0 rounded-2xl bg-destructive/5 opacity-0 group-hover:opacity-100 transition-opacity" />

            <div className="relative">
              <div className="w-14 h-14 mb-4 rounded-xl bg-destructive/10 flex items-center justify-center group-hover:bg-destructive/15 transition-colors">
                <problem.icon className="w-7 h-7 text-destructive" />
              </div>
              <h3 className="font-bold text-text-primary mb-2 text-lg">
                {problem.title}
              </h3>
              <p className="text-sm text-text-muted leading-relaxed">
                {problem.description}
              </p>
            </div>
          </div>
        ))}
      </div>
    </section>
  );
}
