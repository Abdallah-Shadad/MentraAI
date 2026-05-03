import { Mail, Sparkles, Brain } from "lucide-react";

const Footer = () => {
  const links = {
    platform: [
      { name: "Features", href: "#features" },
      { name: "How It Works", href: "#how-it-works" },
      { name: "About Us", href: "#about" },
    ],
    legal: [
      { name: "Privacy Policy", href: "#" },
      { name: "Terms of Service", href: "#" },
    ],
  };

  return (
    <footer
      id="support"
      className="relative bg-background border-t border-border overflow-hidden backdrop-blur-xl"
    >
      <div className="main-container">
        {/* AI decoration */}
        <div className="absolute inset-0 z-0">
          <div className="absolute top-0 left-1/4 w-64 h-64 bg-primary/5 rounded-full blur-[100px]" />
          <div className="absolute bottom-0 right-1/4 w-48 h-48 bg-secondary/5 rounded-full blur-[100px]" />
        </div>

        <div className="relative main-container section-padding pb-8">
          <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-8 mb-12">
            {/* Brand */}
            <div className="sm:col-span-2 lg:col-span-1">
              <div className="flex items-center gap-2 mb-4">
                <div className="relative w-9 h-9 rounded-xl bg-linear-to-br from-primary to-secondary flex items-center justify-center">
                  <Brain className="w-5 h-5 text-foreground" />
                </div>
                <span className="font-heading font-bold text-xl text-text-foreground">
                  Mentra<span className="text-secondary">Ai</span>
                </span>
              </div>
              <p className="text-text-muted text-sm mb-4 leading-relaxed">
                An AI-powered learning platform transforming random learning
                into an organized journey toward professional success
              </p>
            </div>

            {/* Platform Links */}
            <div>
              <h4 className="font-bold mb-4 text-text-foreground">Platform</h4>
              <ul className="space-y-3">
                {links.platform.map((link) => (
                  <li key={link.name}>
                    <a
                      href={link.href}
                      className="text-text-muted hover:text-secondary transition-colors text-sm"
                    >
                      {link.name}
                    </a>
                  </li>
                ))}
              </ul>
            </div>

            {/* Legal Links */}
            <div>
              <h4 className="font-bold mb-4 text-text-foreground">Legal</h4>
              <ul className="space-y-3">
                {links.legal.map((link) => (
                  <li key={link.name}>
                    <a
                      href={link.href}
                      className="text-text-muted hover:text-secondary transition-colors text-sm"
                    >
                      {link.name}
                    </a>
                  </li>
                ))}
              </ul>
            </div>

            {/* Contact */}
            <div>
              <h4 className="font-bold mb-4 text-text-foreground">
                Contact Us
              </h4>
              <p className="text-text-muted text-sm mb-2">support@mentra.ai</p>
              <p className="text-text-muted text-sm">
                Available to answer your questions
              </p>

              {/* AI badge */}
              <div className="mt-4 inline-flex items-center gap-2 px-3 py-1.5 rounded-full bg-secondary/10 border border-secondary/20 text-secondary text-xs">
                <Brain size={12} />
                <span>Powered by AI</span>
              </div>
            </div>
          </div>

          {/* Bottom bar */}
          <div className="pt-8 border-t border-border flex flex-col sm:flex-row items-center justify-between gap-4">
            <p className="text-text-muted text-sm">
              © {new Date().getFullYear()} MentraAi. All rights reserved.
            </p>
            <div className="flex items-center gap-2 text-text-muted text-sm">
              <span>Made with</span>
              <Sparkles size={14} className="text-secondary" />
              <span>for learners everywhere</span>
            </div>
          </div>
        </div>
      </div>
    </footer>
  );
};

export default Footer;
