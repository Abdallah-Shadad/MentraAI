"use client";

import { TrackCard } from "../_components/TrackCard";
import ErrorState from "@/components/reusable/ErrorState";
import SuccessState from "@/components/reusable/SuccessState";

import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogClose,
} from "@/components/ui/dialog";

import {
  Code,
  Server,
  Layers3,
  Smartphone,
  Workflow,
  Cloud,
  Database,
  BarChart3,
  Brain,
  Bot,
  Shield,
  Cpu,
  Gamepad2,
  Blocks,
  Network,
  Bug,
  Terminal,
  Sparkles,
} from "lucide-react";
import { useTrackSelection } from "@/hooks/useCareerTrack";

const tracks = [
  {
    Id: 1,
    Name: "Frontend Engineering",
    Slug: "frontend-engineering",
    icon: <Code className="w-6 h-6 text-foreground" />,
    careers: [
      "Frontend Developer",
      "React Developer",
      "UI Engineer",
      "Web Developer",
    ],
    Description:
      "User interfaces, client-side logic, React, modern web development, and accessibility.",
    IsActive: true,
    CreatedAt: "2026-01-01 00:00:00+00",
  },
  {
    Id: 2,
    Name: "Backend Engineering",
    Slug: "backend-engineering",
    icon: <Server className="w-6 h-6 text-foreground" />,
    careers: [
      "Backend Developer",
      "API Engineer",
      "Software Engineer",
      "Node.js Developer",
    ],
    Description:
      "Server-side logic, robust RESTful/gRPC APIs, microservices architectures, and advanced databases.",
    IsActive: true,
    CreatedAt: "2026-01-01 00:00:00+00",
  },
  {
    Id: 3,
    Name: "Full-Stack Development",
    Slug: "full-stack-development",
    icon: <Layers3 className="w-6 h-6 text-foreground" />,
    careers: [
      "Full-Stack Developer",
      "Software Engineer",
      "Web Developer",
      "Technical Lead",
    ],
    Description:
      "End-to-end applications engineering handling both scalable client-side and server-side components.",
    IsActive: true,
    CreatedAt: "2026-01-01 00:00:00+00",
  },
  {
    Id: 4,
    Name: "Mobile Development (iOS / Android / Cross-platform)",
    Slug: "mobile-development",
    icon: <Smartphone className="w-6 h-6 text-foreground" />,
    careers: [
      "Mobile Developer",
      "Flutter Developer",
      "Android Developer",
      "iOS Developer",
    ],
    Description:
      "Native or cross-platform mobile apps using modern setups like Swift, Kotlin, Flutter, or React Native.",
    IsActive: true,
    CreatedAt: "2026-01-01 00:00:00+00",
  },
  {
    Id: 5,
    Name: "DevOps / Site Reliability Engineering (SRE)",
    Slug: "devops-sre",
    Id: 5,
    icon: <Workflow className="w-6 h-6 text-foreground" />,
    careers: [
      "DevOps Engineer",
      "Site Reliability Engineer",
      "Infrastructure Engineer",
      "Cloud Operations Engineer",
    ],
    Description:
      "Continuous integration, high availability systems, secure cloud delivery, and automation pipelines.",
    IsActive: true,
    CreatedAt: "2026-01-01 00:00:00+00",
  },
  {
    Id: 6,
    Name: "Cloud Architecture / Cloud Engineering",
    Slug: "cloud-engineering",
    icon: <Cloud className="w-6 h-6 text-foreground" />,
    careers: [
      "Cloud Engineer",
      "Cloud Architect",
      "Solutions Architect",
      "Infrastructure Engineer",
    ],
    Description:
      "Designing, managing, and scaling distributed enterprise systems natively across multi-cloud structures.",
    IsActive: true,
    CreatedAt: "2026-01-01 00:00:00+00",
  },
  {
    Id: 7,
    Name: "Data Engineering",
    Slug: "data-engineering",
    icon: <Database className="w-6 h-6 text-foreground" />,
    careers: [
      "Data Engineer",
      "Big Data Engineer",
      "ETL Developer",
      "Analytics Engineer",
    ],
    Description:
      "Constructing robust batch/streaming pipelines, data lakes, warehouses, and data fabric architectures.",
    IsActive: true,
    CreatedAt: "2026-01-01 00:00:00+00",
  },
  {
    Id: 8,
    Name: "Data Science / Analytics",
    Slug: "data-science-analytics",
    icon: <BarChart3 className="w-6 h-6 text-foreground" />,
    careers: [
      "Data Scientist",
      "Data Analyst",
      "Business Analyst",
      "BI Developer",
    ],
    Description:
      "Statistical computing, advanced metrics forecasting, predictive analytics, and deep business intelligence.",
    IsActive: true,
    CreatedAt: "2026-01-01 00:00:00+00",
  },
  {
    Id: 9,
    Name: "Machine Learning Engineering",
    Slug: "machine-learning-engineering",
    icon: <Brain className="w-6 h-6 text-foreground" />,
    careers: [
      "Machine Learning Engineer",
      "AI Engineer",
      "Computer Vision Engineer",
      "NLP Engineer",
    ],
    Description:
      "Building, operationalizing, and fine-tuning advanced predictive models and convolutional architectures.",
    IsActive: true,
    CreatedAt: "2026-01-01 00:00:00+00",
  },
  {
    Id: 10,
    Name: "MLOps / AI Infrastructure",
    Slug: "mlops-ai-infrastructure",
    icon: <Bot className="w-6 h-6 text-foreground" />,
    careers: [
      "MLOps Engineer",
      "AI Platform Engineer",
      "AI Systems Engineer",
      "ML Infrastructure Engineer",
    ],
    Description:
      "Scaling neural network inference pipelines, managing feature stores, and automated model governance.",
    IsActive: true,
    CreatedAt: "2026-01-01 00:00:00+00",
  },
  {
    Id: 11,
    Name: "Cybersecurity Engineering",
    Slug: "cybersecurity-engineering",
    icon: <Shield className="w-6 h-6 text-foreground" />,
    careers: [
      "Cybersecurity Engineer",
      "Security Analyst",
      "SOC Analyst",
      "Penetration Tester",
    ],
    Description:
      "Threat vector emulation, security operations management, application auditing, and defensive infrastructure.",
    IsActive: true,
    CreatedAt: "2026-01-01 00:00:00+00",
  },
  {
    Id: 12,
    Name: "Embedded Systems / IoT",
    Slug: "embedded-systems-iot",
    icon: <Cpu className="w-6 h-6 text-foreground" />,
    careers: [
      "Embedded Engineer",
      "Firmware Engineer",
      "IoT Engineer",
      "Hardware Engineer",
    ],
    Description:
      "Bare-metal or RTOS hardware systems programming, firmware optimization, and microcontrollers architectures.",
    IsActive: true,
    CreatedAt: "2026-01-01 00:00:00+00",
  },
  {
    Id: 13,
    Name: "Game Development",
    Slug: "game-development",
    icon: <Gamepad2 className="w-6 h-6 text-foreground" />,
    careers: [
      "Game Developer",
      "Gameplay Programmer",
      "Game Designer",
      "Unity Developer",
    ],
    Description:
      "Interactive graphics engineering, computer physics simulations, and engine patterns using Unreal or Unity.",
    IsActive: true,
    CreatedAt: "2026-01-01 00:00:00+00",
  },
  {
    Id: 14,
    Name: "Blockchain / Web3 Development",
    Slug: "blockchain-web3",
    icon: <Blocks className="w-6 h-6 text-foreground" />,
    careers: [
      "Blockchain Developer",
      "Smart Contract Engineer",
      "Web3 Developer",
      "Protocol Engineer",
    ],
    Description:
      "Smart contracts protocol development, cryptographical consensus algorithms, and decentralized operations.",
    IsActive: true,
    CreatedAt: "2026-01-01 00:00:00+00",
  },
  {
    Id: 15,
    Name: "Platform Engineering",
    Slug: "platform-engineering",
    icon: <Network className="w-6 h-6 text-foreground" />,
    careers: [
      "Platform Engineer",
      "Infrastructure Engineer",
      "Systems Engineer",
      "Developer Experience Engineer",
    ],
    Description:
      "Building automated Internal Developer Platforms (IDP) and scaling standard tool chains for product teams.",
    IsActive: true,
    CreatedAt: "2026-01-01 00:00:00+00",
  },
  {
    Id: 16,
    Name: "QA / Test Automation Engineering",
    Slug: "qa-test-automation",
    icon: <Bug className="w-6 h-6 text-foreground" />,
    careers: [
      "QA Engineer",
      "Automation Engineer",
      "Software Test Engineer",
      "QA Lead",
    ],
    Description:
      "Architecting end-to-end integration test suites, behavior testing, and continuous quality guards.",
    IsActive: true,
    CreatedAt: "2026-01-01 00:00:00+00",
  },
  {
    Id: 17,
    Name: "Systems Programming",
    Slug: "systems-programming",
    icon: <Terminal className="w-6 h-6 text-foreground" />,
    careers: [
      "Systems Programmer",
      "Rust Developer",
      "C++ Engineer",
      "Compiler Engineer",
    ],
    Description:
      "Low-level resource managers, memory allocators, compilers infrastructure development using Rust, C, or C++.",
    IsActive: true,
    CreatedAt: "2026-01-01 00:00:00+00",
  },
  {
    Id: 18,
    Name: "AI / LLM Application Development",
    Slug: "ai-llm-application-development",
    icon: <Sparkles className="w-6 h-6 text-foreground" />,
    careers: [
      "AI Application Developer",
      "LLM Engineer",
      "Generative AI Engineer",
      "AI Solutions Engineer",
    ],
    Description:
      "Architecting agentic setups, prompt frameworks pipelines, vector stores indexing, and cognitive workflow flows.",
    IsActive: true,
    CreatedAt: "2026-01-01 00:00:00+00",
  },
];

export default function TracksList({ isOpen, onOpenChange }) {
  const {
    mutate: postTrackSelection,
    isPending,
    isError,
    error,
    isSuccess,
    reset,
    data,
  } = useTrackSelection();

  if (isError) {
    return (
      <ErrorState
        message={
          error?.response?.data?.error?.message ||
          "An error occurred while selecting the track"
        }
        close={() => {
          reset();
        }}
      />
    );
  }

  if (isSuccess) {
    return (
      <SuccessState
        message="Track selected successfully"
        close={() => {
          reset();
          onOpenChange(false);
        }}
      />
    );
  }

  return (
    <Dialog open={isOpen} onOpenChange={onOpenChange}>
      <DialogContent className="min-w-[calc(100%-10rem)] h-[95vh] overflow-y-auto border-border [&>button]:text-destructive [&>button]:hover:text-destructive-foreground [&>button]:cursor-pointer">
        <DialogHeader>
          <DialogTitle className="text-2xl font-bold text-center text-foreground">
            Select a Track
          </DialogTitle>
          <DialogDescription className="text-foreground-muted text-center">
            Choose a track to start your learning journey
          </DialogDescription>
        </DialogHeader>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mt-6">
          {tracks.map((track) => (
            <TrackCard
              key={track.Name}
              {...track}
              onOpenChange={onOpenChange}
              postTrackSelection={postTrackSelection}
            />
          ))}
        </div>
      </DialogContent>
    </Dialog>
  );
}
