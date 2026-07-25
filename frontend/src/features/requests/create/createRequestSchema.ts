import { z } from "zod";

export const createRequestSchema = z.object({
  title: z.string().trim().min(5).max(100),
  description: z.string().trim().min(10).max(2000),
  category: z.enum([
    "Equipment",
    "Training",
    "SoftwareAccess",
    "Expense",
    "Leave",
    "Other",
  ]),
});

export type CreateRequestForm = z.infer<typeof createRequestSchema>;