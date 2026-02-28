"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/features/auth/hooks/useAuth";

export default function UserDetailsPage() {
  const router = useRouter();
  const { user, fetchUser, loading, logout } = useAuth();

  useEffect(() => {
    // Only fetch user if not already loaded
    if (!user) {
      fetchUser().catch(() => {
        router.replace("/login"); // redirect if not authenticated
      });
    }
  }, [user, fetchUser, router]);

  if (loading) {
    return <p className="text-center mt-10">Loading...</p>;
  }

  if (!user) {
    return null; // waiting for fetch or redirect
  }

  const handleLogout = async () => {
    try {
      await logout();
      router.replace("/login"); // redirect after logout
    } catch (err) {
      console.error("Logout failed:", err);
    }
  };
  return (
    <div className="flex items-center justify-center min-h-screen bg-gray-100">
      <div className="w-full max-w-md p-8 bg-white rounded shadow">
        <h1 className="text-2xl font-bold mb-6 text-center">User Details</h1>

        <p><strong>ID:</strong> {user.id}</p>
        <p><strong>Name:</strong> {user.firstName} {user.lastName}</p>
        <p><strong>Email:</strong> {user.email}</p>
        <button
          onClick={handleLogout}
          className="mt-6 w-full py-2 px-4 bg-red-600 text-white rounded hover:bg-red-700"
        >
          Logout
        </button>
      </div>
    </div>
  );
}