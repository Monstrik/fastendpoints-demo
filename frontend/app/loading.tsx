export default function Loading() {
  return (
    <section className="page-shell" aria-busy="true" aria-live="polite">
      <div className="page-heading-skeleton" />
      <div className="page-subheading-skeleton" />
      <div className="content-skeleton-grid">
        <div className="content-skeleton-card" />
        <div className="content-skeleton-card" />
        <div className="content-skeleton-card" />
      </div>
    </section>
  );
}

