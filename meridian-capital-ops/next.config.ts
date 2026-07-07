import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Emit .next/standalone for the Docker image (see Dockerfile / ../infra).
  output: "standalone",
};

export default nextConfig;
