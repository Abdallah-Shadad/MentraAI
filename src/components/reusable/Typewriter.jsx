"use client";

import { useEffect, useState } from "react";

export default function Typewriter({ text = "Hello World", className }) {
  const [displayText, setDisplayText] = useState("");
  const [isDeleting, setIsDeleting] = useState(false);

  useEffect(() => {
    let timeout;

    if (!isDeleting) {
      // الكتابة
      if (displayText.length < text.length) {
        timeout = setTimeout(() => {
          setDisplayText(text.slice(0, displayText.length + 1));
        }, 150);
      } else {
        timeout = setTimeout(() => {
          setIsDeleting(true);
        }, 1000);
      }
    } else {
      // المسح
      if (displayText.length > 0) {
        timeout = setTimeout(() => {
          setDisplayText(text.slice(0, displayText.length - 1));
        }, 50);
      } else {
        setIsDeleting(false);
      }
    }

    return () => clearTimeout(timeout);
  }, [displayText, isDeleting, text]);

  return <span className={className}>{displayText}</span>;
}
