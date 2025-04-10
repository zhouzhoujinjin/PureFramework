import PropTypes from 'prop-types'
import React from 'react'

const MentionComponent = ({ mention, children, className }) => (
  <span className={className} spellCheck={false} data-role-id={mention.id}>
    {children}
  </span>
)

MentionComponent.propTypes = {
  mention: PropTypes.shape({
    id: PropTypes.string
  }).isRequired,
  className: PropTypes.string
}

export default MentionComponent
