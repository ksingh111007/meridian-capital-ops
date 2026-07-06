import type { Metadata } from "next";
import { getPortalTax } from "@/lib/data";
import { PortalTaxScreen } from "@/screens/portal/PortalTaxScreen";

export const metadata: Metadata = { title: "Tax Documents · Investor Portal" };

/** Screen 6h — K-1s and tax packages by year. */
export default function PortalTaxPage() {
  const { banner, documents } = getPortalTax();
  return <PortalTaxScreen banner={banner} documents={documents} />;
}
