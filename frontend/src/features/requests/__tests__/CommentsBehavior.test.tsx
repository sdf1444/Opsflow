import { Alert, Stack } from "@mui/material";
import { useState } from "react";
import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { AddCommentForm } from "../components/AddCommentForm";
import { CommentsList } from "../components/CommentsList";
import { buildComment } from "../../../test/dataBuilders";
import { renderWithProviders } from "../../../test/render";

function CommentsHarness({ onAdd }: { onAdd: (content: string) => void }) {
  const [comments, setComments] = useState([buildComment()]);

  return (
    <Stack spacing={2}>
      <AddCommentForm
        isSubmitting={false}
        onSubmit={(content) => {
          onAdd(content);
          setComments((current) => [
            ...current,
            buildComment({ id: `comment-${current.length + 1}`, content }),
          ]);
        }}
      />
      {comments.length === 0 ? <Alert severity="info">No comments</Alert> : null}
      <CommentsList comments={comments} />
    </Stack>
  );
}

describe("Comments behavior", () => {
  it("shows empty state when no comments", () => {
    renderWithProviders(<CommentsList comments={[]} />);

    expect(screen.getByText("No comments yet.")).toBeInTheDocument();
  });

  it("calls mutation handler and shows new comment", async () => {
    const user = userEvent.setup();
    const onAdd = vi.fn();

    renderWithProviders(<CommentsHarness onAdd={onAdd} />);

    await user.type(screen.getByLabelText("Comment"), "Please review this today");
    await user.click(screen.getByRole("button", { name: "Post comment" }));

    expect(onAdd).toHaveBeenCalledWith("Please review this today");
    expect(screen.getByText("Please review this today")).toBeInTheDocument();
  });
});
