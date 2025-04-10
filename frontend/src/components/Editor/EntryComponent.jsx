import PropTypes from 'prop-types'
import React from 'react'

const EntryComponent = ({
  mention,
  theme,
  searchValue, // eslint-disable-line no-unused-vars
  isFocused, // eslint-disable-line no-unused-vars
  ...parentProps
}) => {
  return (
    <div {...parentProps}>
      <div className={theme.mentionSuggestionsEntryContainer}>
        <div data-role-id={`${mention.id}`} className={theme.mentionSuggestionsEntryContainerRight}>
          <div className={theme.mentionSuggestionsEntryText}>{mention.name}</div>
        </div>
      </div>
    </div>
  )
}

EntryComponent.propTypes = {
  mention: PropTypes.shape({
    name: PropTypes.string,
    mentionId: PropTypes.string
  }).isRequired,
  theme: PropTypes.shape({
    mentionSuggestionsEntryContainer: PropTypes.string,
    mentionSuggestionsEntryContainerRight: PropTypes.string,
    mentionSuggestionsEntryText: PropTypes.string
  }).isRequired,
  // eslint-disable-next-line react/require-default-props
  searchValue: PropTypes.string,
  isFocused: PropTypes.bool
}

export default EntryComponent
