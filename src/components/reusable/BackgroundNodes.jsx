"use client";
import { useEffect, useRef } from "react";

export const BackgroundNodes = () => {
  const canvasRef = useRef(null);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    const ctx = canvas.getContext("2d");
    if (!ctx) return;

    let animationId;
    const nodes = [];
    const numNodes = 50;
    const connectionDistance = 150;

    const resize = () => {
      canvas.width = window.innerWidth;
      canvas.height = window.innerHeight;
    };

    const initNodes = () => {
      nodes.length = 0;
      for (let i = 0; i < numNodes; i++) {
        nodes.push({
          x: Math.random() * canvas.width,
          y: Math.random() * canvas.height,
          vx: (Math.random() - 0.5) * 0.5,
          vy: (Math.random() - 0.5) * 0.5,
          radius: Math.random() * 2 + 1,
        });
      }
    };

    const draw = () => {
      ctx.clearRect(0, 0, canvas.width, canvas.height);

      // Update and draw nodes
      nodes.forEach((node) => {
        node.x += node.vx;
        node.y += node.vy;

        // Bounce off edges
        if (node.x < 0 || node.x > canvas.width) node.vx *= -1;
        if (node.y < 0 || node.y > canvas.height) node.vy *= -1;

        // Draw node
        ctx.beginPath();
        ctx.arc(node.x, node.y, node.radius, 0, Math.PI * 2);
        ctx.fillStyle = "rgba(139, 92, 246, 0.5)";
        ctx.fill();
      });

      // Draw connections
      nodes.forEach((nodeA, i) => {
        nodes.slice(i + 1).forEach((nodeB) => {
          const dx = nodeA.x - nodeB.x;
          const dy = nodeA.y - nodeB.y;
          const distance = Math.sqrt(dx * dx + dy * dy);

          if (distance < connectionDistance) {
            const opacity = (1 - distance / connectionDistance) * 0.15;
            ctx.beginPath();
            ctx.moveTo(nodeA.x, nodeA.y);
            ctx.lineTo(nodeB.x, nodeB.y);
            ctx.strokeStyle = `rgba(139, 92, 246, ${opacity})`;
            ctx.lineWidth = 1;
            ctx.stroke();
          }
        });
      });

      animationId = requestAnimationFrame(draw);
    };

    resize();
    initNodes();
    draw();

    const handleResize = () => {
      resize();
      initNodes();
    };

    window.addEventListener("resize", handleResize);

    return () => {
      cancelAnimationFrame(animationId);
      window.removeEventListener("resize", handleResize);
    };
  }, []);

  return (
    <canvas
      ref={canvasRef}
      className="absolute inset-0 pointer-events-none z-0 opacity-50"
    />
  );
};
