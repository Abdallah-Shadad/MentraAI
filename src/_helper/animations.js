/********** animations **********/

// transition function to avoid code repetition
const transitionFn = (duration, delay) => ({
  duration,
  delay,
  ease: "easeInOut",
});

function fromLeft({ duration = 0.5, delay = 0 }) {
  return {
    initial: { x: "-100%" },
    animate: { x: "0%", transition: transitionFn(duration, delay) },
    exit: { x: "-100%", transition: transitionFn(duration, delay) },
  };
}

function fromRight({ duration = 0.5, delay = 0 }) {
  return {
    initial: { x: "100%", opacity: 0 },
    animate: {
      x: "0%",
      opacity: 1,
      transition: transitionFn(duration, delay),
    },
    exit: {
      x: "100%",
      opacity: 0,
      transition: transitionFn(duration, delay),
    },
  };
}

function fromTop({ duration = 0.5, delay = 0 }) {
  return {
    initial: { y: "-70px", opacity: 0, scale: 0.9 },
    animate: {
      y: "0px",
      opacity: 1,
      scale: 1,
      transition: transitionFn(duration, delay),
    },
    exit: {
      y: "-70px",
      opacity: 0,
      scale: 0.9,
      transition: transitionFn(duration, delay),
    },
  };
}

function zoomout({ duration = 0.5, delay = 0 }) {
  return {
    initial: { scale: 0.2, opacity: 0.2 },
    animate: {
      scale: 1,
      opacity: 1,
      transition: transitionFn(duration, delay),
    },
  };
}

function toleft({ duration = 0.5, delay = 0 }) {
  return {
    initial: { x: "100%", opacity: 0 },
    animate: {
      x: "0%",
      opacity: 1,
      transition: transitionFn(duration, delay),
    },
    exit: {
      x: "-100%",
      opacity: 0,
      transition: transitionFn(duration, delay),
    },
  };
}

export { fromLeft, fromRight, fromTop, zoomout, toleft };
