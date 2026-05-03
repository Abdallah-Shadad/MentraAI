"use client";

import { TrackCard } from "../_components/TrackCard";

import { Code, Database, Brain, Shield, Gamepad2, Palette } from "lucide-react";

const tracks = [
  {
    name: "Frontend Development",
    icon: <Code className="w-6 h-6 text-primary" />,
    level: "Beginner",
    duration: "4-6 months",
    careers: ["Web Developer", "UI Engineer"],
    learnPoints: [
      "HTML, CSS, JavaScript",
      "React & TypeScript",
      "Responsive Design",
    ],
  },
  {
    name: "Data Science",
    icon: <Database className="w-6 h-6 text-secondary" />,
    level: "Intermediate",
    duration: "6-8 months",
    careers: ["Data Analyst", "Data Scientist"],
    learnPoints: [
      "Python & Pandas",
      "Machine Learning basics",
      "Data Visualization",
    ],
  },
  {
    name: "AI & Machine Learning",
    icon: <Brain className="w-6 h-6 text-accent" />,
    level: "Intermediate",
    duration: "8-12 months",
    careers: ["ML Engineer", "AI Researcher"],
    learnPoints: [
      "Neural Networks",
      "TensorFlow & PyTorch",
      "NLP & Computer Vision",
    ],
  },
  {
    name: "Cybersecurity",
    icon: <Shield className="w-6 h-6 text-success" />,
    level: "Beginner",
    duration: "5-7 months",
    careers: ["Security Analyst", "Pentester"],
    learnPoints: ["Network Security", "Ethical Hacking", "Security Auditing"],
  },
  {
    name: "Game Development",
    icon: <Gamepad2 className="w-6 h-6 text-chart-5" />,
    level: "Beginner",
    duration: "6-9 months",
    careers: ["Game Developer", "Game Designer"],
    learnPoints: ["Unity or Unreal", "Game Physics", "3D Modeling basics"],
  },
  {
    name: "UX/UI Design",
    icon: <Palette className="w-6 h-6 text-chart-4" />,
    level: "Beginner",
    duration: "3-5 months",
    careers: ["UX Designer", "Product Designer"],
    learnPoints: ["Design Principles", "Figma & Prototyping", "User Research"],
  },
];

export default function TracksList({ onSelectTrack }) {
  return (
    <div className="animate-fade-in-up">
      <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
        {tracks.map((track) => (
          <TrackCard
            key={track.name}
            {...track}
            onClick={() => onSelectTrack(track.name)}
          />
        ))}
      </div>
    </div>
  );
}
