import { motion } from "framer-motion";
import { MdError } from "react-icons/md";
import { zoomout } from "./animations";
import { useState, useEffect, useRef } from "react";

// error component
function Error({ error }) {
  return (
    <motion.div
      variants={zoomout({ duration: 0.3, delay: 0 })}
      initial="initial"
      whileInView="animate"
      className="bg-error/50 text-red-600  p-2 rounded-lg font-semibold flex items-center gap-2 "
    >
      <MdError />
      {error}
    </motion.div>
  );
}

// counter component
function Counter({ target, duration }) {
  const [count, setCount] = useState(0);
  const [visible, setVisible] = useState(false);
  const ref = useRef();

  useEffect(() => {
    const observer = new IntersectionObserver(
      ([entry]) => entry.isIntersecting && setVisible(true),
      { threshold: 0.5 }
    );
    observer.observe(ref.current);
    return () => observer.disconnect();
  }, []);

  useEffect(() => {
    if (!visible) return;
    let start = 0;
    const increment = target / (duration / 10);
    const timer = setInterval(() => {
      start += increment;
      if (start >= target) {
        start = target;
        clearInterval(timer);
      }
      setCount(Math.floor(start));
    }, 10);

    return () => clearInterval(timer);
  }, [visible, target, duration]);

  return (
    <h1 ref={ref} className="text-3xl font-semibold text-white">
      {count} +
    </h1>
  );
}

//to top
function ToTop() {
  useEffect(() => {
    window.scrollTo({
      top: 0,
      left: 0,
      behavior: "smooth",
    });
  }, []);
  return <></>;
}
export { Error, Counter, ToTop };
