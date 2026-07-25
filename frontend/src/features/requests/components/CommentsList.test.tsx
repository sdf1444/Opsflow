import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { CommentsList } from "./CommentsList";

describe("CommentsList", () => {
  it("shows empty state", () => {
    render(<CommentsList comments={[]} />);

    expect(screen.getByText("No comments yet.")).toBeInTheDocument();
  });
});
