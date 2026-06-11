"use client";
import { useState, useEffect } from "react";
import { useUpdateUserProfile } from "@/hooks/useUser";
import { Loader2, Save, Sparkles, BookOpen, Briefcase, Plus, X } from "lucide-react";
import ErrorState from "@/components/reusable/ErrorState";
import SuccessState from "@/components/reusable/SuccessState";

const ageOptions = [
  "18-24 years old",
  "25-34 years old",
  "35-44 years old",
  "45-54 years old",
  "55-64 years old",
  "65 years or older",
  "Prefer not to say",
];

const edLevelOptions = [
  "Primary/elementary school",
  "Secondary school (e.g. American high school, German Realschule or Gymnasium, etc.)",
  "Some college/university study without earning a degree",
  "Associate degree (A.A., A.S., etc.)",
  "Bachelor's degree (B.A., B.S., B.Eng., etc.)",
  "Master's degree (M.A., M.S., M.Eng., MBA, etc.)",
  "Professional degree (JD, MD, Ph.D, Ed.D, etc.)",
  "Other (please specify):",
];

const employmentOptions = [
  "Employed",
  "Independent contractor, freelancer, or self-employed",
  "Student",
  "Not employed",
  "I prefer not to say",
];

const remoteWorkOptions = [
  "Remote",
  "Hybrid (some in-person, leans heavy to flexibility)",
  "Hybrid (some remote, leans heavy to in-person)",
  "Your choice (very flexible, you can come in when you want or just as needed)",
  "In-person",
];

const industryOptions = [
  "Software Development",
  "Computer Systems Design and Services",
  "Internet, Telecomm or Information Services",
  "Fintech",
  "Banking/Financial Services",
  "Insurance",
  "Healthcare",
  "Retail and Consumer Services",
  "Manufacturing",
  "Transportation, or Supply Chain",
  "Energy",
  "Government",
  "Higher Education",
  "Media & Advertising Services",
  "Other:",
];

const orgSizeOptions = [
  "Just me - I am a freelancer, sole proprietor, etc.",
  "Less than 20 employees",
  "20 to 99 employees",
  "100 to 499 employees",
  "500 to 999 employees",
  "1,000 to 4,999 employees",
  "5,000 to 9,999 employees",
  "10,000 or more employees",
  "I don't know",
];

const aiSelectOptions = [
  "Yes, I use AI tools daily",
  "Yes, I use AI tools weekly",
  "Yes, I use AI tools monthly or infrequently",
  "No, but I plan to soon",
  "No, and I don't plan to",
];

// Helper to map DB/onboarding options to the validator-expected values
const mapDbToForm = (key, val) => {
  if (!val) return "";
  const s = String(val).trim();
  
  if (key === "edLevel") {
    if (s.includes("Primary")) return "Primary/elementary school";
    if (s.includes("Secondary")) return "Secondary school (e.g. American high school, German Realschule or Gymnasium, etc.)";
    if (s.includes("Some college")) return "Some college/university study without earning a degree";
    if (s.includes("Associate")) return "Associate degree (A.A., A.S., etc.)";
    if (s.includes("Bachelor")) return "Bachelor's degree (B.A., B.S., B.Eng., etc.)";
    if (s.includes("Master")) return "Master's degree (M.A., M.S., M.Eng., MBA, etc.)";
    if (s.includes("Professional")) return "Professional degree (JD, MD, Ph.D, Ed.D, etc.)";
    if (s.includes("Other")) return "Other (please specify):";
  }
  if (key === "employment") {
    if (s.includes("Independent") || s.includes("freelancer")) return "Independent contractor, freelancer, or self-employed";
  }
  if (key === "remoteWork") {
    if (s === "Hybrid") return "Hybrid (some in-person, leans heavy to flexibility)";
  }
  if (key === "industry") {
    if (s === "Retail") return "Retail and Consumer Services";
    if (s === "Education") return "Higher Education";
    if (s === "Other") return "Other:";
  }
  if (key === "orgSize") {
    if (s === "Just me") return "Just me - I am a freelancer, sole proprietor, etc.";
    if (s === "1,000+ employees") return "1,000 to 4,999 employees";
  }
  if (key === "aiSelect") {
    if (s === "true" || s === "Yes") return "Yes, I use AI tools daily";
    if (s === "false" || s === "No") return "No, and I don't plan to";
  }
  return s;
};

export default function ProfileEditForm({ profile }) {
  const { mutate: updateProfile, isPending, isError, error, isSuccess, reset } = useUpdateUserProfile();
  
  // Local Form state
  const [formData, setFormData] = useState({
    age: "",
    edLevel: "",
    yearsCode: 0,
    workExp: 0,
    employment: "",
    remoteWork: "",
    industry: "",
    orgSize: "",
    aiSelect: "",
  });

  const [currentSkills, setCurrentSkills] = useState([]);
  const [futureSkills, setFutureSkills] = useState([]);
  const [currentSkillInput, setCurrentSkillInput] = useState("");
  const [futureSkillInput, setFutureSkillInput] = useState("");

  const [successMessage, setSuccessMessage] = useState("");

  useEffect(() => {
    if (profile) {
      setFormData({
        age: profile.age || "",
        edLevel: mapDbToForm("edLevel", profile.edLevel),
        yearsCode: profile.yearsCode || 0,
        workExp: profile.workExp || 0,
        employment: mapDbToForm("employment", profile.employment),
        remoteWork: mapDbToForm("remoteWork", profile.remoteWork),
        industry: mapDbToForm("industry", profile.industry),
        orgSize: mapDbToForm("orgSize", profile.orgSize),
        aiSelect: mapDbToForm("aiSelect", profile.aiSelect),
      });
      setCurrentSkills(profile.currentSkills || []);
      setFutureSkills(profile.futureSkills || []);
    }
  }, [profile]);

  useEffect(() => {
    if (isSuccess) {
      setSuccessMessage("Your profile information has been successfully updated!");
      const timer = setTimeout(() => {
        setSuccessMessage("");
        reset();
      }, 4000);
      return () => clearTimeout(timer);
    }
  }, [isSuccess, reset]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: e.target.type === "number" ? (value === "" ? 0 : Number(value)) : value,
    }));
  };

  const handleAddSkill = (type) => {
    if (type === "current") {
      const trimmed = currentSkillInput.trim();
      if (trimmed && !currentSkills.includes(trimmed)) {
        setCurrentSkills((prev) => [...prev, trimmed]);
      }
      setCurrentSkillInput("");
    } else {
      const trimmed = futureSkillInput.trim();
      if (trimmed && !futureSkills.includes(trimmed)) {
        setFutureSkills((prev) => [...prev, trimmed]);
      }
      setFutureSkillInput("");
    }
  };

  const handleRemoveSkill = (type, skillToRemove) => {
    if (type === "current") {
      setCurrentSkills((prev) => prev.filter((s) => s !== skillToRemove));
    } else {
      setFutureSkills((prev) => prev.filter((s) => s !== skillToRemove));
    }
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    const payload = {
      ...formData,
      currentSkills: currentSkills && currentSkills.length > 0 ? currentSkills : null,
      futureSkills: futureSkills && futureSkills.length > 0 ? futureSkills : null,
    };
    
    // Replace empty string values with null if they represent nullable database enums
    Object.keys(payload).forEach((key) => {
      if (payload[key] === "") {
        payload[key] = null;
      }
    });

    updateProfile(payload);
  };

  const errorMessage = error?.response?.data?.error?.message || "An error occurred while saving profile changes.";

  return (
    <form onSubmit={handleSubmit} className="bg-card/50 backdrop-blur-sm border border-border rounded-lg p-6 shadow-shadow-card space-y-8">
      {isError && (
        <ErrorState
          message={errorMessage}
          close={() => reset()}
        />
      )}

      {successMessage && (
        <SuccessState
          message={successMessage}
          close={() => setSuccessMessage("")}
        />
      )}

      {/* Grid Container */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
        
        {/* Section 1: Professional & Education Experience */}
        <div className="space-y-5">
          <h5 className="text-base font-bold text-primary flex items-center gap-2 border-b border-border/40 pb-2">
            <BookOpen className="w-4.5 h-4.5" />
            <span>Education & Experience</span>
          </h5>

          <div className="space-y-2">
            <label className="text-xs font-semibold text-foreground-secondary tracking-wide uppercase">
              Age Bracket
            </label>
            <select
              name="age"
              value={formData.age}
              onChange={handleChange}
              className="w-full bg-bg-surface border border-border rounded-md px-3.5 py-2.5 text-sm text-foreground outline-none focus:ring-2 focus:ring-primary/40 focus:border-transparent transition-all"
            >
              <option value="">Select Age</option>
              {ageOptions.map((opt) => (
                <option key={opt} value={opt}>{opt}</option>
              ))}
            </select>
          </div>

          <div className="space-y-2">
            <label className="text-xs font-semibold text-foreground-secondary tracking-wide uppercase">
              Education Level
            </label>
            <select
              name="edLevel"
              value={formData.edLevel}
              onChange={handleChange}
              className="w-full bg-bg-surface border border-border rounded-md px-3.5 py-2.5 text-sm text-foreground outline-none focus:ring-2 focus:ring-primary/40 focus:border-transparent transition-all"
            >
              <option value="">Select Level</option>
              {edLevelOptions.map((opt) => (
                <option key={opt} value={opt}>{opt}</option>
              ))}
            </select>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <label className="text-xs font-semibold text-foreground-secondary tracking-wide uppercase">
                Coding Years
              </label>
              <input
                type="number"
                name="yearsCode"
                min="0"
                max="60"
                value={formData.yearsCode}
                onChange={handleChange}
                className="w-full bg-bg-surface border border-border rounded-md px-3.5 py-2.5 text-sm text-foreground outline-none focus:ring-2 focus:ring-primary/40 focus:border-transparent transition-all"
              />
            </div>
            <div className="space-y-2">
              <label className="text-xs font-semibold text-foreground-secondary tracking-wide uppercase">
                Work Experience
              </label>
              <input
                type="number"
                name="workExp"
                min="0"
                max="60"
                value={formData.workExp}
                onChange={handleChange}
                className="w-full bg-bg-surface border border-border rounded-md px-3.5 py-2.5 text-sm text-foreground outline-none focus:ring-2 focus:ring-primary/40 focus:border-transparent transition-all"
              />
            </div>
          </div>

          <div className="space-y-2">
            <label className="text-xs font-semibold text-foreground-secondary tracking-wide uppercase">
              Employment Status
            </label>
            <select
              name="employment"
              value={formData.employment}
              onChange={handleChange}
              className="w-full bg-bg-surface border border-border rounded-md px-3.5 py-2.5 text-sm text-foreground outline-none focus:ring-2 focus:ring-primary/40 focus:border-transparent transition-all"
            >
              <option value="">Select Status</option>
              {employmentOptions.map((opt) => (
                <option key={opt} value={opt}>{opt}</option>
              ))}
            </select>
          </div>
        </div>

        {/* Section 2: Work Environment & Preferences */}
        <div className="space-y-5">
          <h5 className="text-base font-bold text-secondary flex items-center gap-2 border-b border-border/40 pb-2">
            <Briefcase className="w-4.5 h-4.5" />
            <span>Work Environment & Preferences</span>
          </h5>

          <div className="space-y-2">
            <label className="text-xs font-semibold text-foreground-secondary tracking-wide uppercase">
              Remote Work Choice
            </label>
            <select
              name="remoteWork"
              value={formData.remoteWork}
              onChange={handleChange}
              className="w-full bg-bg-surface border border-border rounded-md px-3.5 py-2.5 text-sm text-foreground outline-none focus:ring-2 focus:ring-secondary/40 focus:border-transparent transition-all"
            >
              <option value="">Select Choice</option>
              {remoteWorkOptions.map((opt) => (
                <option key={opt} value={opt}>{opt}</option>
              ))}
            </select>
          </div>

          <div className="space-y-2">
            <label className="text-xs font-semibold text-foreground-secondary tracking-wide uppercase">
              Industry Sector
            </label>
            <select
              name="industry"
              value={formData.industry}
              onChange={handleChange}
              className="w-full bg-bg-surface border border-border rounded-md px-3.5 py-2.5 text-sm text-foreground outline-none focus:ring-2 focus:ring-secondary/40 focus:border-transparent transition-all"
            >
              <option value="">Select Industry</option>
              {industryOptions.map((opt) => (
                <option key={opt} value={opt}>{opt}</option>
              ))}
            </select>
          </div>

          <div className="space-y-2">
            <label className="text-xs font-semibold text-foreground-secondary tracking-wide uppercase">
              Organization Size
            </label>
            <select
              name="orgSize"
              value={formData.orgSize}
              onChange={handleChange}
              className="w-full bg-bg-surface border border-border rounded-md px-3.5 py-2.5 text-sm text-foreground outline-none focus:ring-2 focus:ring-secondary/40 focus:border-transparent transition-all"
            >
              <option value="">Select Size</option>
              {orgSizeOptions.map((opt) => (
                <option key={opt} value={opt}>{opt}</option>
              ))}
            </select>
          </div>

          <div className="space-y-2">
            <label className="text-xs font-semibold text-foreground-secondary tracking-wide uppercase">
              AI Tools Frequency
            </label>
            <select
              name="aiSelect"
              value={formData.aiSelect}
              onChange={handleChange}
              className="w-full bg-bg-surface border border-border rounded-md px-3.5 py-2.5 text-sm text-foreground outline-none focus:ring-2 focus:ring-secondary/40 focus:border-transparent transition-all"
            >
              <option value="">Select Frequency</option>
              {aiSelectOptions.map((opt) => (
                <option key={opt} value={opt}>{opt}</option>
              ))}
            </select>
          </div>
        </div>
      </div>

      {/* Section 3: Skills Inventory */}
      <div className="border-t border-border/40 pt-6 space-y-6">
        <h5 className="text-base font-bold text-foreground flex items-center gap-2">
          <Sparkles className="w-4.5 h-4.5 text-primary" />
          <span>Skills Inventory</span>
        </h5>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
          {/* Current Skills */}
          <div className="space-y-3">
            <label className="text-xs font-semibold text-foreground-secondary tracking-wide uppercase">
              Current Skills
            </label>
            <div className="flex gap-2">
              <input
                type="text"
                placeholder="Add skill (e.g. React)"
                value={currentSkillInput}
                onChange={(e) => setCurrentSkillInput(e.target.value)}
                onKeyDown={(e) => e.key === "Enter" && (e.preventDefault(), handleAddSkill("current"))}
                className="w-full bg-bg-surface border border-border rounded-md px-3.5 py-2 text-sm text-foreground outline-none focus:ring-1 focus:ring-primary/40 transition-all"
              />
              <button
                type="button"
                onClick={() => handleAddSkill("current")}
                className="p-2 border border-border hover:border-foreground-muted rounded-md text-foreground bg-surface-elevated/40 cursor-pointer"
              >
                <Plus className="w-5 h-5" />
              </button>
            </div>
            
            <div className="flex flex-wrap gap-2 min-h-12 p-3 bg-bg-surface/50 border border-border rounded-md">
              {currentSkills.length === 0 ? (
                <span className="text-xs text-foreground-muted/50 my-auto">No skills added yet.</span>
              ) : (
                currentSkills.map((skill) => (
                  <span
                    key={skill}
                    className="inline-flex items-center gap-1.5 text-xs font-medium px-2.5 py-1 rounded bg-primary/15 text-primary-light border border-primary/25"
                  >
                    <span>{skill}</span>
                    <button
                      type="button"
                      onClick={() => handleRemoveSkill("current", skill)}
                      className="text-foreground-muted hover:text-foreground cursor-pointer"
                    >
                      <X className="w-3.5 h-3.5" />
                    </button>
                  </span>
                ))
              )}
            </div>
          </div>

          {/* Future Skills */}
          <div className="space-y-3">
            <label className="text-xs font-semibold text-foreground-secondary tracking-wide uppercase">
              Future Skills (To Learn)
            </label>
            <div className="flex gap-2">
              <input
                type="text"
                placeholder="Add skill (e.g. Next.js)"
                value={futureSkillInput}
                onChange={(e) => setFutureSkillInput(e.target.value)}
                onKeyDown={(e) => e.key === "Enter" && (e.preventDefault(), handleAddSkill("future"))}
                className="w-full bg-bg-surface border border-border rounded-md px-3.5 py-2 text-sm text-foreground outline-none focus:ring-1 focus:ring-secondary/40 transition-all"
              />
              <button
                type="button"
                onClick={() => handleAddSkill("future")}
                className="p-2 border border-border hover:border-foreground-muted rounded-md text-foreground bg-surface-elevated/40 cursor-pointer"
              >
                <Plus className="w-5 h-5" />
              </button>
            </div>
            
            <div className="flex flex-wrap gap-2 min-h-12 p-3 bg-bg-surface/50 border border-border rounded-md">
              {futureSkills.length === 0 ? (
                <span className="text-xs text-foreground-muted/50 my-auto">No goals added yet.</span>
              ) : (
                futureSkills.map((skill) => (
                  <span
                    key={skill}
                    className="inline-flex items-center gap-1.5 text-xs font-medium px-2.5 py-1 rounded bg-secondary/15 text-secondary-light border border-secondary/25"
                  >
                    <span>{skill}</span>
                    <button
                      type="button"
                      onClick={() => handleRemoveSkill("future", skill)}
                      className="text-foreground-muted hover:text-foreground cursor-pointer"
                    >
                      <X className="w-3.5 h-3.5" />
                    </button>
                  </span>
                ))
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Submit Action */}
      <div className="flex justify-end pt-4 border-t border-border/40">
        <button
          type="submit"
          disabled={isPending}
          className="flex items-center justify-center gap-2 px-6 py-3.5 bg-primary/20 border border-primary hover:bg-primary text-foreground font-bold rounded-md shadow-shadow-neon transition-all active:scale-[0.98] cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed w-full sm:w-auto"
        >
          {isPending ? (
            <>
              <Loader2 className="w-5 h-5 animate-spin" />
              <span>Saving Changes...</span>
            </>
          ) : (
            <>
              <Save className="w-5 h-5" />
              <span>Save Changes</span>
            </>
          )}
        </button>
      </div>
    </form>
  );
}
