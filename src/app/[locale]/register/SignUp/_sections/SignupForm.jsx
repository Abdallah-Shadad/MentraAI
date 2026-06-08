"use client";
import { useState, useEffect } from "react";
//components
import ErrorState from "@/components/reusable/ErrorState";
//icons
import { TriangleAlert, Eye, EyeOff, IdCard, Loader2 } from "lucide-react";

//hooks
import { useRegister } from "@/hooks/useAuth";

export default function SignupForm({ setSteps }) {
  const {
    mutate: register,
    isSuccess,
    isError,
    error,
    data,
    isPending,
    reset,
  } = useRegister();

  const [form, setForm] = useState({
    firstName: "",
    lastName: "",
    email: "",
    password: "",
    confirmPassword: "",
  });
  const [errors, setErrors] = useState({});
  const [showPassword, setShowPassword] = useState(false);

  const strength = getStrength(form.password);

  const validate = () => {
    const e = {};

    if (!form.firstName) e.firstName = "Required";
    if (!form.lastName) e.lastName = "Required";
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email))
      e.email = "Invalid email";
    if (checkPasswordStrength(form.password) < 3)
      e.password =
        "Password must be at least 8 characters with numbers, small letters, capital letters and symbols";
    if (form.confirmPassword !== form.password)
      e.confirmPassword = "Passwords do not match";

    setErrors(e);
    return Object.keys(e).length === 0;
  };

  //handle submit
  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!validate()) return;
    register(form);
  };

  useEffect(() => {
    if (isSuccess) {
      setSteps(2);
    }
  }, [isSuccess, setSteps]);

  const inputClass = (name) =>
    `w-full bg-background text-foreground placeholder:text-foreground-muted border ${
      errors[name] ? "border-red-500" : "border-border"
    } rounded-lg px-3 py-3 text-sm focus:outline-none focus:ring-2 focus:ring-purple-500`;

  return (
    <div className="font-sans w-full shrink-0 relative">
      {isError && (
        <ErrorState
          message={error?.response?.data?.error?.message}
          close={() => reset()}
        />
      )}
      <div className="bg-card/70 border border-border rounded-2xl shadow-2xl w-full p-4">
        <form>
          {/* Personal */}
          <p className="text-xs md:text-sm text-foreground uppercase my-6 flex items-center gap-2">
            <IdCard className="w-5 h-5 inline shrink-0" />{" "}
            <span>Account Information</span>
          </p>

          {/* name */}
          <div className="grid md:grid-cols-2 gap-4">
            <div>
              <input
                placeholder="First Name"
                className={inputClass("firstName")}
                onChange={(e) =>
                  setForm({ ...form, firstName: e.target.value })
                }
              />
              {errors.firstName && (
                <p className="text-destructive text-xs mt-1 flex items-center gap-1">
                  <span className="text-red-500">
                    <TriangleAlert className="w-4 h-4 inline" />
                  </span>{" "}
                  {errors.firstName}
                </p>
              )}
            </div>

            <div>
              <input
                placeholder="Last Name"
                className={inputClass("lastName")}
                onChange={(e) => setForm({ ...form, lastName: e.target.value })}
              />
              {errors.lastName && (
                <p className="text-destructive text-xs mt-2 flex items-center gap-1">
                  <span className="text-red-500">
                    <TriangleAlert className="w-4 h-4 inline" />
                  </span>{" "}
                  {errors.lastName}
                </p>
              )}
            </div>
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
          <div className="grid md:grid-cols-1 gap-4 my-6">
            <div>
              <div className="relative">
                <input
                  type={showPassword ? "text" : "password"}
                  placeholder="Password"
                  className={inputClass("password")}
                  onChange={(e) =>
                    setForm({ ...form, password: e.target.value })
                  }
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-foreground-muted hover:text-foreground"
                >
                  {showPassword ? (
                    <Eye className="w-5 h-5" />
                  ) : (
                    <EyeOff className="w-5 h-5" />
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
              {errors.password && (
                <p className="text-destructive text-xs mt-2 flex items-center gap-1">
                  <span className="text-red-500">
                    <TriangleAlert className="w-4 h-4 inline" />
                  </span>{" "}
                  {errors.password}
                </p>
              )}
            </div>

            <div>
              <div className="relative">
                <input
                  type={showPassword ? "text" : "password"}
                  placeholder="Confirm Password"
                  className={inputClass("confirmPassword")}
                  onChange={(e) =>
                    setForm({ ...form, confirmPassword: e.target.value })
                  }
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-foreground-muted hover:text-foreground"
                >
                  {showPassword ? (
                    <Eye className="w-5 h-5" />
                  ) : (
                    <EyeOff className="w-5 h-5" />
                  )}
                </button>
              </div>
              {errors.confirmPassword && (
                <p className="text-destructive text-xs mt-2 flex items-center gap-1">
                  <span className="text-red-500">
                    <TriangleAlert className="w-4 h-4 inline" />
                  </span>{" "}
                  {errors.confirmPassword}
                </p>
              )}
            </div>
          </div>
        </form>

        {/* button movement */}
        <div className="w-full flex gap-2">
          <button
            onClick={handleSubmit}
            className="group cursor-pointer w-full mt-6 py-3 rounded-md bg-linear-to-r from-purple-500 to-purple-700 text-white font-semibold hover:opacity-90 disabled:opacity-50"
          >
            {isPending ? (
              <span className="flex items-center justify-center gap-2">
                <Loader2 className="w-5 h-5 animate-spin" />
                Second Step
              </span>
            ) : (
              <span>First Step</span>
            )}
          </button>
        </div>
      </div>
    </div>
  );
}

// password strength
function checkPasswordStrength(password) {
  const v = password;
  let score = 0;
  if (v.length >= 8) score++;
  if (/[A-Z]/.test(v)) score++;
  if (/[^A-Za-z0-9]/.test(v)) score++;
  return score;
}

// password strength
function getStrength(password) {
  const score = checkPasswordStrength(password);

  if (!password) return { width: "0%", label: "", color: "" };
  if (score <= 1)
    return { width: "25%", label: "Weak", color: "bg-red-500 text-red-400" };
  if (score === 2)
    return {
      width: "50%",
      label: "Fair",
      color: "bg-yellow-500 text-yellow-400",
    };
  return {
    width: "100%",
    label: "Strong",
    color: "bg-success text-success",
  };
}
