import { motion } from "framer-motion";

export default function Header() {
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
