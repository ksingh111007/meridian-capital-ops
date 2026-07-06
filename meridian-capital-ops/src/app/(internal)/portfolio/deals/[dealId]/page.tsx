import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { getDeal, getDeals } from "@/lib/data";
import { DealDetailScreen } from "@/screens/DealDetailScreen";

export async function generateMetadata({ params }: { params: Promise<{ dealId: string }> }): Promise<Metadata> {
  const { dealId } = await params;
  return { title: getDeal(dealId)?.name ?? "Deal" };
}

export function generateStaticParams() {
  return getDeals().map((d) => ({ dealId: d.id }));
}

/** Screen 6d — deal detail: terms, cashflows, risk & LP exposure. */
export default async function DealDetailPage({ params }: { params: Promise<{ dealId: string }> }) {
  const { dealId } = await params;
  const deal = getDeal(dealId);
  if (!deal) notFound();
  return <DealDetailScreen deal={deal} />;
}
