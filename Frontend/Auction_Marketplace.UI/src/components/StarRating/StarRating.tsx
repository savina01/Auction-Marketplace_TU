import React from "react";
import "./StarRating.css"; 

const StarRating: React.FC<{ rating: number }> = ({ rating }) => {
  const stars = [];
  for (let i = 0; i < 5; i++) {
    if (i < rating) {
      stars.push(<span key={i} className="star-rating">&#9733;</span>); 
    } else {
      stars.push(<span key={i} className="star-rating">&#9734;</span>);
    }
  }
  return <div>{stars}</div>;
};

export default StarRating;
