"use client";
import { useState } from "react";
import { ArrowRight, TriangleAlert, Eye, EyeOff } from "lucide-react";

export default function SignupForm({ steps, setSteps }) {
  const [form, setForm] = useState({
    fname: "",
    lname: "",
    gender: "",
    email: "",
    pass: "",
    pass2: "",
    terms: false,
  });

  const [errors, setErrors] = useState({});
  const [success, setSuccess] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  // password strength
  const getStrength = () => {
    const v = form.pass;
    let score = 0;
    if (v.length >= 8) score++;
    if (/[A-Z]/.test(v)) score++;
    if (/[0-9]/.test(v)) score++;
    if (/[^A-Za-z0-9]/.test(v)) score++;

    if (!v) return { width: "0%", label: "", color: "" };
    if (score <= 1)
      return { width: "25%", label: "Weak", color: "bg-red-500 text-red-400" };
    if (score === 2)
      return {
        width: "50%",
        label: "Fair",
        color: "bg-yellow-500 text-yellow-400",
      };
    if (score === 3)
      return {
        width: "75%",
        label: "Good",
        color: "bg-blue-500 text-blue-400",
      };
    return {
      width: "100%",
      label: "Strong",
      color: "bg-success text-success",
    };
  };

  const strength = getStrength();

  const validate = () => {
    const e = {};

    if (!form.fname) e.fname = "Required";
    if (!form.lname) e.lname = "Required";
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email))
      e.email = "Invalid email";
    if (form.pass.length < 8)
      e.pass =
        "Password must be at least 8 characters with numbers, small and capital letters and symbols";
    if (form.pass2 !== form.pass) e.pass2 = "Passwords do not match";
    if (!form.terms) e.terms = "You must accept terms";

    setErrors(e);
    return Object.keys(e).length === 0;
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!validate()) return;

    setLoading(true);
    setTimeout(() => {
      setSuccess(true);
      setLoading(false);
    }, 1400);
  };

  const inputClass = (name) =>
    `w-full bg-[#1a1a2e] border ${
      errors[name] ? "border-red-500" : "border-slate-800"
    } rounded-md px-3 py-2 text-sm text-slate-100 focus:outline-none focus:ring-2 focus:ring-purple-500`;

  if (success) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-bg-card/50">
        <div className="text-center">
          <div className="text-5xl text-success mb-2">✦</div>
          <h2 className="text-xl font-semibold text-white">
            Welcome to MentarAI!
          </h2>
          <p className="text-slate-400 text-sm mt-1">
            Your account has been created.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="font-sans w-full shrink-0">
      <div className="bg-bg-card/70 border border-slate-800 rounded-2xl shadow-2xl w-full p-4">
        <form onSubmit={handleSubmit}>
          {/* Personal */}
          <p className="text-xs text-purple-300 uppercase tracking-widest mb-4">
            Personal Information
          </p>

          {/* name */}
          <div className="grid md:grid-cols-2 gap-4">
            <div>
              <input
                placeholder="First Name"
                className={inputClass("fname")}
                onChange={(e) => setForm({ ...form, fname: e.target.value })}
              />
              {errors.fname && (
                <p className="text-destructive text-xs mt-1 flex items-center gap-1">
                  <span className="text-red-500">
                    <TriangleAlert className="w-4 h-4 inline" />
                  </span>{" "}
                  {errors.fname}
                </p>
              )}
            </div>

            <div>
              <input
                placeholder="Last Name"
                className={inputClass("lname")}
                onChange={(e) => setForm({ ...form, lname: e.target.value })}
              />
              {errors.lname && (
                <p className="text-destructive text-xs mt-2 flex items-center gap-1">
                  <span className="text-red-500">
                    <TriangleAlert className="w-4 h-4 inline" />
                  </span>{" "}
                  {errors.lname}
                </p>
              )}
            </div>
          </div>

          {/* gender */}
          <div className="flex gap-2 mt-4">
            {["male", "female"].map((g) => (
              <label
                key={g}
                className={`flex-1 flex items-center gap-2 px-3 py-2 rounded-md border cursor-pointer ${
                  form.gender === g
                    ? "border-purple-500 bg-purple-500/10"
                    : "border-slate-800"
                }`}
              >
                <input
                  type="radio"
                  name="gender"
                  onChange={() => setForm({ ...form, gender: g })}
                />
                <span className="text-sm text-slate-300 capitalize">{g}</span>
              </label>
            ))}
          </div>

          {/* email */}
          <div className="mt-6">
            <input
              placeholder="Email"
              className={inputClass("email")}
              onChange={(e) => setForm({ ...form, email: e.target.value })}
            />
            {errors.email && (
              <p className="text-destructive text-xs mt-2 flex items-center gap-1">
                <span className="text-red-500">
                  <TriangleAlert className="w-4 h-4 inline" />
                </span>{" "}
                {errors.email}
              </p>
            )}
          </div>

          {/* password */}
          <div className="grid md:grid-cols-2 gap-4 my-6">
            <div>
              <div className="relative">
                <input
                  type={showPassword ? "text" : "password"}
                  placeholder="Password"
                  className={inputClass("pass")}
                  onChange={(e) => setForm({ ...form, pass: e.target.value })}
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-200"
                >
                  {showPassword ? (
                    <EyeOff className="w-5 h-5" />
                  ) : (
                    <Eye className="w-5 h-5" />
                  )}
                </button>
              </div>

              {/* strength */}
              <div className="mt-2">
                <div className="h-2 bg-[#1a1a2e] rounded">
                  <div
                    className={`h-2 rounded ${strength.color.split(" ")[0]}`}
                    style={{ width: strength.width }}
                  />
                </div>
                <p className={`text-xs mt-1 ${strength.color.split(" ")[1]}`}>
                  {strength.label}
                </p>
              </div>
              {errors.pass && (
                <p className="text-destructive text-xs mt-2 flex items-center gap-1">
                  <span className="text-red-500">
                    <TriangleAlert className="w-4 h-4 inline" />
                  </span>{" "}
                  {errors.pass}
                </p>
              )}
            </div>

            <div>
              <div className="relative">
                <input
                  type={showPassword ? "text" : "password"}
                  placeholder="Confirm Password"
                  className={inputClass("pass2")}
                  onChange={(e) => setForm({ ...form, pass2: e.target.value })}
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-200"
                >
                  {showPassword ? (
                    <EyeOff className="w-5 h-5" />
                  ) : (
                    <Eye className="w-5 h-5" />
                  )}
                </button>
              </div>
              {errors.pass2 && (
                <p className="text-destructive text-xs mt-2 flex items-center gap-1">
                  <span className="text-red-500">
                    <TriangleAlert className="w-4 h-4 inline" />
                  </span>{" "}
                  {errors.pass2}
                </p>
              )}
            </div>
          </div>

          {/* checkbox */}
          <div>
            <div className="flex items-start gap-2 mt-6">
              <input
                type="checkbox"
                onChange={(e) => setForm({ ...form, terms: e.target.checked })}
              />
              <span className="text-sm text-slate-300">
                I agree to Terms & Privacy
              </span>
            </div>

            {errors.terms && (
              <p className="text-destructive text-xs mt-2 flex items-center gap-1">
                <span className="text-red-500">
                  <TriangleAlert className="w-4 h-4 inline" />
                </span>{" "}
                {errors.terms}
              </p>
            )}
          </div>
        </form>
        {/* button movement */}
        <div className="w-full flex gap-2">
          <button
            onClick={() => {
              if (validate()) {
                setSteps(steps + 1);
              }
            }}
            className="group cursor-pointer w-full mt-6 py-3 rounded-md bg-linear-to-r from-purple-500 to-purple-700 text-white font-semibold hover:opacity-90 disabled:opacity-50"
          >
            <span>
              Next{" "}
              <ArrowRight className="inline ml-1 w-5 h-5 group-hover:translate-x-1 transition-transform" />
            </span>
          </button>
        </div>
      </div>
    </div>
  );
}
