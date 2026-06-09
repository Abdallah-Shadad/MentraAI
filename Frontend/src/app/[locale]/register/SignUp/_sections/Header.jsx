import { motion } from "framer-motion";

export default function Header({ steps }) {
  if (steps === 1) {
    return (
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        exit={{ opacity: 0, y: -20 }}
        transition={{ duration: 0.5 }}
        className="text-center mb-4"
      >
        <h1 className="text-xl md:text-2xl lg:text-4xl font-bold text-foreground mb-2">
          Student Registration
        </h1>
        <p className="text-sm md:text-base text-foreground-muted">
          Join the AI-powered learning platform
        </p>
      </motion.div>
    );
  }
  if (steps > 1) {
    return (
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        exit={{ opacity: 0, y: -20 }}
        transition={{ duration: 0.5 }}
        className="text-center mb-4"
      >
        <h1 className="text-xl md:text-2xl lg:text-4xl font-bold flex flex-col gap-2 text-foreground mb-2">
          <span>Not sure where to start?</span>
          <span className="primary-gradient">
            We'll help you figure it out.
          </span>
        </h1>
        <p className="text-sm md:text-base text-foreground-muted">
          Choose your own path, or let AI recommend the best learning journey
          for you.
        </p>
      </motion.div>
    );
  }

  return null;
}
